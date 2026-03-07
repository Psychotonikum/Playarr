using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download.Aggregation.Aggregators;
using Playarr.Core.Indexers;
using Playarr.Core.Indexers.TorrentRss;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Download.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguagesFixture : CoreTest<AggregateLanguages>
    {
        private RemoteEpisode _remoteRom;
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

            _remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                 .With(l => l.ParsedRomInfo = null)
                                                 .With(l => l.Roms = roms)
                                                 .With(l => l.Game = _series)
                                                 .With(l => l.Release = new ReleaseInfo())
                                                 .Build();
        }

        private ParsedRomInfo GetParsedRomInfo(List<Language> languages, string releaseTitle, string releaseTokens = "")
        {
            return new ParsedRomInfo
                   {
                       Languages = languages,
                       ReleaseTitle = releaseTitle,
                       ReleaseTokens = releaseTokens
                   };
        }

        [Test]
        public void should_return_existing_language_if_episode_title_does_not_have_language()
        {
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Original }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteRom).Languages.Should().Contain(_series.OriginalLanguage);
        }

        [Test]
        public void should_return_parsed_language()
        {
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteRom).Languages.Should().Equal(_remoteRom.ParsedRomInfo.Languages);
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_when_indexer_id_has_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_from_indexer_with_id_when_indexer_id_and_name_are_set(string releaseTitle)
        {
            var indexerDefinition1 = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer1",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            var indexerDefinition2 = new IndexerDefinition
            {
                Id = 2,
                Name = "MyIndexer2",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.German.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition1);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition1, indexerDefinition2 });

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Indexer = "MyIndexer2";
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_when_indexer_name_has_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer (Prowlarr)",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.FindByName("MyIndexer (Prowlarr)"))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.Indexer = "MyIndexer (Prowlarr)";
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.FindByName("MyIndexer (Prowlarr)"), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_when_release_as_unknown_as_default_language_and_indexer_has_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Unknown }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_when_release_as_specified_language_and_indexer_has_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.French }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_multi_languages_when_release_as_other_language_and_indexer_has_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.German }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French, Language.German });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_original_when_indexer_has_no_multi_languages_configuration(string releaseTitle)
        {
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(indexerDefinition);

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.IndexerId = 1;
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [TestCase("Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup")]
        [TestCase("Game Title (2025) [COMPLETA] [1080p H265 EAC3 MultiLang MultiSub][RlsGroup]")]
        [TestCase("Game Title - Stagione 1 (2025) [COMPLETA] 720p H264 MULTILANG AAC 2.0 MULTISUB-RlsGroup")]
        [TestCase("Game Title (2007) S01E01 [Multilang AC3 Sub Spa Eng Rus]")]
        public void should_return_original_when_no_indexer_value(string releaseTitle)
        {
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage });
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_exclude_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Game.Title.S01E01.Jimmy.The.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.xyz-RlsGroup";

            _remoteRom.Roms.First().Title = "Jimmy The Greek";
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteRom).Languages.Should().Equal(_series.OriginalLanguage);
        }

        [Test]
        public void should_remove_parsed_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Game.Title.S01E01.Jimmy.The.Greek.French.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.French.xyz-RlsGroup";

            _remoteRom.Roms.First().Title = "Jimmy The Greek";
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Greek, Language.French }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteRom).Languages.Should().Equal(Language.French);
        }

        [Test]
        public void should_not_exclude_language_that_is_part_of_episode_title_when_release_tokens_does_not_contain_episode_title()
        {
            var releaseTitle = "Game.Title.S01E01.xyz-RlsGroup";
            var releaseTokens = ".xyz-RlsGroup";

            _remoteRom.Roms.First().Title = "Jimmy The Greek";
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteRom).Languages.Should().Equal(Language.Greek);
        }

        [Test]
        public void should_use_reparse_language_after_determining_languages_that_are_in_episode_titles()
        {
            var releaseTitle = "Game.Title.S01E01.Jimmy.The.Greek.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.Greek.xyz-RlsGroup";

            _remoteRom.Roms.First().Title = "Jimmy The Greek";
            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteRom).Languages.Should().Equal(Language.Greek);
        }

        [Test]
        public void should_return_multi_languages_from_indexer_with_name_when_indexer_id_does_not_exist()
        {
            var releaseTitle = "Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition1 = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer1",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            var indexerDefinition2 = new IndexerDefinition
            {
                Id = 2,
                Name = "MyIndexer2",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.German.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(null as IndexerDefinition);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.FindByName("MyIndexer1"))
                .Returns(indexerDefinition1);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition1, indexerDefinition2 });

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.IndexerId = 10;
            _remoteRom.Release.Indexer = "MyIndexer1";
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(10), Times.Once());
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.FindByName("MyIndexer1"), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_multi_languages_from_indexer_with_name_when_indexer_id_not_available()
        {
            var releaseTitle = "Game.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition1 = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer1",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            var indexerDefinition2 = new IndexerDefinition
            {
                Id = 2,
                Name = "MyIndexer2",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.German.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(1))
                .Returns(null as IndexerDefinition);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.FindByName("MyIndexer1"))
                .Returns(indexerDefinition1);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition1, indexerDefinition2 });

            _remoteRom.ParsedRomInfo = GetParsedRomInfo(new List<Language> { }, releaseTitle);
            _remoteRom.Release.IndexerId = 0;
            _remoteRom.Release.Indexer = "MyIndexer1";
            _remoteRom.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteRom).Languages.Should().BeEquivalentTo(new List<Language> { _series.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Find(10), Times.Never());
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.FindByName("MyIndexer1"), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }
    }
}
