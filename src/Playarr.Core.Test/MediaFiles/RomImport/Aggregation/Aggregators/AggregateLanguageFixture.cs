using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguageFixture : CoreTest<AggregateLanguage>
    {
        private LocalEpisode _localRom;
        private Game _series;
        private string _simpleReleaseTitle = "Game.Title.S01E01.xyz-RlsGroup";

        [SetUp]
        public void Setup()
        {
            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .BuildList();

            _series = Builder<Game>.CreateNew()
                       .With(m => m.OriginalLanguage = Language.English)
                       .Build();

            _localRom = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.DownloadClientRomInfo = null)
                                                 .With(l => l.FolderRomInfo = null)
                                                 .With(l => l.FileRomInfo = null)
                                                 .With(l => l.Roms = roms)
                                                 .With(l => l.Game = _series)
                                                 .Build();
        }

        private void GivenAugmenters(List<Language> fileNameLanguages, List<Language> folderNameLanguages, List<Language> clientLanguages, List<Language> mediaInfoLanguages)
        {
            var fileNameAugmenter = new Mock<IAugmentLanguage>();
            var folderNameAugmenter = new Mock<IAugmentLanguage>();
            var clientInfoAugmenter = new Mock<IAugmentLanguage>();
            var mediaInfoAugmenter = new Mock<IAugmentLanguage>();

            fileNameAugmenter.Setup(s => s.AugmentLanguage(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                   .Returns(new AugmentLanguageResult(fileNameLanguages, Confidence.Filename));

            folderNameAugmenter.Setup(s => s.AugmentLanguage(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                   .Returns(new AugmentLanguageResult(folderNameLanguages, Confidence.Foldername));

            clientInfoAugmenter.Setup(s => s.AugmentLanguage(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                   .Returns(new AugmentLanguageResult(clientLanguages, Confidence.DownloadClientItem));

            mediaInfoAugmenter.Setup(s => s.AugmentLanguage(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                   .Returns(new AugmentLanguageResult(mediaInfoLanguages, Confidence.MediaInfo));

            var mocks = new List<Mock<IAugmentLanguage>> { fileNameAugmenter, folderNameAugmenter, clientInfoAugmenter, mediaInfoAugmenter };

            Mocker.SetConstant<IEnumerable<IAugmentLanguage>>(mocks.Select(c => c.Object));
        }

        private ParsedRomInfo GetParsedRomInfo(List<Language> languages, string releaseTitle)
        {
            return new ParsedRomInfo
                   {
                       Languages = languages,
                       ReleaseTitle = releaseTitle
                   };
        }

        [Test]
        public void should_return_default_if_no_info_is_known()
        {
            var result = Subject.Aggregate(_localRom, null);

            result.Languages.Should().Contain(_series.OriginalLanguage);
        }

        [Test]
        public void should_return_file_language_when_only_file_info_is_known()
        {
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            GivenAugmenters(new List<Language> { Language.French },
                null,
                null,
                null);

            Subject.Aggregate(_localRom, null).Languages.Should().Equal(_localRom.FileRomInfo.Languages);
        }

        [Test]
        public void should_return_folder_language_when_folder_info_is_known()
        {
            _localRom.FolderRomInfo = GetParsedRomInfo(new List<Language> { Language.German }, _simpleReleaseTitle);
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            GivenAugmenters(new List<Language> { Language.French },
                new List<Language> { Language.German },
                null,
                null);

            Subject.Aggregate(_localRom, null).Languages.Should().Equal(_localRom.FolderRomInfo.Languages);
        }

        [Test]
        public void should_return_download_client_item_language_when_download_client_item_info_is_known()
        {
            _localRom.DownloadClientRomInfo = GetParsedRomInfo(new List<Language> { Language.Spanish }, _simpleReleaseTitle);
            _localRom.FolderRomInfo = GetParsedRomInfo(new List<Language> { Language.German }, _simpleReleaseTitle);
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            GivenAugmenters(new List<Language> { Language.French },
                new List<Language> { Language.German },
                new List<Language> { Language.Spanish },
                null);

            Subject.Aggregate(_localRom, null).Languages.Should().Equal(_localRom.DownloadClientRomInfo.Languages);
        }

        [Test]
        public void should_return_file_language_when_file_language_is_higher_than_others()
        {
            _localRom.DownloadClientRomInfo = GetParsedRomInfo(new List<Language> { Language.Unknown }, _simpleReleaseTitle);
            _localRom.FolderRomInfo = GetParsedRomInfo(new List<Language> { Language.Unknown }, _simpleReleaseTitle);
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            GivenAugmenters(new List<Language> { Language.French },
                new List<Language> { Language.Unknown },
                new List<Language> { Language.Unknown },
                null);

            Subject.Aggregate(_localRom, null).Languages.Should().Contain(_localRom.FileRomInfo.Languages);
        }

        [Test]
        public void should_return_multi_language()
        {
            GivenAugmenters(new List<Language> { Language.Unknown },
                            new List<Language> { Language.French, Language.German },
                            new List<Language> { Language.Unknown },
                            null);

            Subject.Aggregate(_localRom, null).Languages.Should().Equal(new List<Language> { Language.French, Language.German });
        }

        [Test]
        public void should_use_mediainfo_over_others()
        {
            GivenAugmenters(new List<Language> { Language.Unknown },
                            new List<Language> { Language.French, Language.German },
                            new List<Language> { Language.Unknown },
                            new List<Language> { Language.Japanese, Language.English });

            Subject.Aggregate(_localRom, null).Languages.Should().Equal(new List<Language> { Language.Japanese, Language.English });
        }

        [Test]
        public void should_return_english_if_parsed_language_is_in_episode_title_and_release_title_contains_episode_title()
        {
            _localRom.Roms.First().Title = "The Swedish Job";
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.Swedish }, "Game.Title.S01E01.The.Swedish.Job.720p.WEB-DL-RlsGrp");

            GivenAugmenters(new List<Language> { },
                            null,
                            null,
                            null);

            Subject.Aggregate(_localRom, null).Languages.Should().Contain(Language.English);
        }

        [Test]
        public void should_return_parsed_if_parsed_language_is_not_episode_title_and_release_title_contains_episode_title()
        {
            _localRom.Roms.First().Title = "The Swedish Job";
            _localRom.FileRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, "Game.Title.S01E01.The.Swedish.Job.720p.WEB-DL-RlsGrp");

            GivenAugmenters(new List<Language> { Language.French },
                null,
                null,
                null);

            Subject.Aggregate(_localRom, null).Languages.Should().Contain(_localRom.FileRomInfo.Languages);
        }
    }
}
