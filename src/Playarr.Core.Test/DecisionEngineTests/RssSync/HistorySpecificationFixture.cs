using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.History;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.CustomFormats;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const int SECOND_EPISODE_ID = 2;

        private HistorySpecification _upgradeHistory;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Tuple<QualityModel, Language> _upgradableQuality;
        private Tuple<QualityModel, Language> _notupgradableQuality;
        private Game _fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            CustomFormatsTestHelpers.GivenCustomFormats();

            var singleEpisodeList = new List<Rom> { new Rom { Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Rom>
            {
                                                            new Rom { Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 },
                                                            new Rom { Id = SECOND_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 4 },
                                                            new Rom { Id = 3, SeasonNumber = 12, EpisodeNumber = 5 }
            };

            _fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.Bluray1080p.Id,
                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Game = _fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Roms = doubleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            _parseResultSingle = new RemoteEpisode
            {
                Game = _fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Roms = singleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.SDTV, new Revision(version: 1)), Language.English);

            _notupgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.HDTV1080p, new Revision(version: 2)), Language.English);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeHistory>(), It.IsAny<Game>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenMostRecentForEpisode(int romId, string downloadId, Tuple<QualityModel, Language> quality, DateTime date, EpisodeHistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(romId))
                  .Returns(new EpisodeHistory { DownloadId = downloadId, Quality = quality.Item1, Date = date, EventType = eventType, Languages = new List<Language> { quality.Item2 } });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SeasonSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>())).Returns((EpisodeHistory)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

// [Test]
//        public void should_return_true_if_latest_history_has_a_download_id_and_cdh_is_enabled()
//        {
//            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
//            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
//        }

        [Test]
        public void should_return_true_if_latest_history_item_is_older_than_twelve_hours()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-13), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.English);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_episode_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }
    }
}
