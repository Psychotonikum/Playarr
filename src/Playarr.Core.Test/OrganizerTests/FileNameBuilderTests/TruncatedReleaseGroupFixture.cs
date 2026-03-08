using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.MediaFiles;
using Playarr.Core.Organizer;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedReleaseGroupFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private List<Rom> _episodes;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "Game Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.MultiEpisodeStyle = 0;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episodes = new List<Rom>
                        {
                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Rom Title 1")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.EpisodeNumber = 1)
                                            .Build()
                        };

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "PlayarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _romFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_truncate_from_beginning()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _romFile.Quality.Quality = Quality.Bluray1080p;
            _romFile.ReleaseGroup = "IWishIWasALittleBitTallerIWishIWasABallerIWishIHadAGirlWhoLookedGoodIWouldCallHerIWishIHadARabbitInAHatWithABatAndASixFourImpala";
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}-{ReleaseGroup:12}";

            var result = Subject.BuildFileName(_episodes, _series, _romFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("The Fantastic Life of Mr. Sisko - S01E01 - Rom Title 1 Bluray-1080p-IWishIWas....mkv");
        }

        [Test]
        public void should_truncate_from_from_end()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _romFile.Quality.Quality = Quality.Bluray1080p;
            _romFile.ReleaseGroup = "IWishIWasALittleBitTallerIWishIWasABallerIWishIHadAGirlWhoLookedGoodIWouldCallHerIWishIHadARabbitInAHatWithABatAndASixFourImpala";
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}-{ReleaseGroup:-17}";

            var result = Subject.BuildFileName(_episodes, _series, _romFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("The Fantastic Life of Mr. Sisko - S01E01 - Rom Title 1 Bluray-1080p-...ASixFourImpala.mkv");
        }
    }
}
