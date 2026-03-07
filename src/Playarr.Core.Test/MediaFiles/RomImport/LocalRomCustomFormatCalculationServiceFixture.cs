using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class LocalEpisodeCustomFormatCalculationServiceFixture : CoreTest<LocalEpisodeCustomFormatCalculationService>
    {
        private const int EnglishCustomFormatScore = 10;
        private const int SpanishCustomFormatScore = 2;
        private LocalEpisode _localRom;
        private Game _series;
        private QualityModel _quality;
        private CustomFormat _englishCustomFormat;
        private CustomFormat _spanishCustomFormat;

        [SetUp]
        public void Setup()
        {
            _englishCustomFormat = new CustomFormat("HasEnglish") { Id = 1 };
            _spanishCustomFormat = new CustomFormat("HasSpanish") { Id = 2 };
            _series = Builder<Game>.CreateNew()
                                     .With(e => e.Path = @"C:\Test\Game".AsOsAgnostic())
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                         FormatItems = [
                                             new ProfileFormatItem { Format = _englishCustomFormat, Score = EnglishCustomFormatScore },
                                             new ProfileFormatItem { Format = _spanishCustomFormat, Score = SpanishCustomFormatScore }
                                         ]
                                     })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localRom = new LocalEpisode
            {
                Game = _series,
                Quality = _quality,
                Languages = new List<Language> { Language.Spanish },
                Roms = new List<Rom> { new Rom() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi"
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalEpisode>(), It.Is<string>(x => x.Contains("English"))))
                .Returns([_englishCustomFormat]);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalEpisode>(), It.Is<string>(x => x.Contains("Spanish"))))
                .Returns([_spanishCustomFormat]);
        }

        [Test]
        public void should_build_a_filename_and_use_it_to_calculate_custom_score()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<List<Rom>>(), It.IsAny<Game>(), It.IsAny<RomFile>(), "", null, null))
                .Returns(renamedFileName);

            Subject.ParseEpisodeCustomFormats(_localRom).Should().BeEquivalentTo([_englishCustomFormat]);
        }

        [Test]
        public void should_update_custom_formats_on_local_episode()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<List<Rom>>(), It.IsAny<Game>(), It.IsAny<RomFile>(), "", null, null))
                .Returns(renamedFileName);

            Subject.UpdateEpisodeCustomFormats(_localRom);
            _localRom.FileNameUsedForCustomFormatCalculation.Should().Be(renamedFileName);

            _localRom.OriginalFileNameCustomFormats.Should().BeEquivalentTo([_spanishCustomFormat]);
            _localRom.OriginalFileNameCustomFormatScore.Should().Be(SpanishCustomFormatScore);

            _localRom.CustomFormats.Should().BeEquivalentTo([_englishCustomFormat]);
            _localRom.CustomFormatScore.Should().Be(EnglishCustomFormatScore);
        }
    }
}
