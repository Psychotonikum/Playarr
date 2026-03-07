using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.DecisionEngine.Specifications.RssSync;
using Playarr.Core.Download.Pending;
using Playarr.Core.Indexers;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DelaySpecificationFixture : CoreTest<DelaySpecification>
    {
        private QualityProfile _profile;
        private DelayProfile _delayProfile;
        private RemoteEpisode _remoteRom;

        [SetUp]
        public void Setup()
        {
            _profile = Builder<QualityProfile>.CreateNew()
                                       .Build();

            _delayProfile = Builder<DelayProfile>.CreateNew()
                                                 .With(d => d.PreferredProtocol = DownloadProtocol.Usenet)
                                                 .Build();

            var game = Builder<Game>.CreateNew()
                                        .With(s => s.QualityProfile = _profile)
                                        .Build();

            _remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Game = game)
                                                   .Build();

            _profile.Items = new List<QualityProfileQualityItem>();
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.HDTV720p });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.WEBDL720p });
            _profile.Items.Add(new QualityProfileQualityItem { Allowed = true, Quality = Quality.Bluray720p });

            _profile.Cutoff = Quality.WEBDL720p.Id;

            _remoteRom.ParsedRomInfo = new ParsedRomInfo();
            _remoteRom.Release = new ReleaseInfo();
            _remoteRom.Release.DownloadProtocol = DownloadProtocol.Usenet;

            _remoteRom.Roms = Builder<Rom>.CreateListOfSize(1).Build().ToList();
            _remoteRom.Roms.First().EpisodeFileId = 0;

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);

            Mocker.GetMock<IPendingReleaseService>()
                  .Setup(s => s.GetPendingRemoteEpisodes(It.IsAny<int>()))
                  .Returns(new List<RemoteEpisode>());
        }

        private void GivenExistingFile(QualityModel quality, Language language)
        {
            _remoteRom.Roms.First().EpisodeFileId = 1;

            _remoteRom.Roms.First().RomFile = new RomFile
            {
                Quality = quality,
                Languages = new List<Language> { language },
                SceneName = "Game.Title.S01E01.720p.HDTV.x264-Playarr"
            };
        }

        private void GivenUpgradeForExistingFile()
        {
            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsUpgradable(It.IsAny<QualityProfile>(), It.IsAny<QualityModel>(), It.IsAny<List<CustomFormat>>(), It.IsAny<QualityModel>(), It.IsAny<List<CustomFormat>>()))
                  .Returns(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_be_true_when_user_invoked_search()
        {
            Subject.IsSatisfiedBy(new RemoteEpisode(), new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria { UserInvokedSearch = true })).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_system_invoked_search_and_release_is_younger_than_delay()
        {
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.SDTV);
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new ReleaseDecisionInformation(false, new SingleEpisodeSearchCriteria())).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_profile_does_not_have_a_delay()
        {
            _delayProfile.UsenetDelay = 0;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_quality_and_language_is_last_allowed_in_profile_and_bypass_disabled()
        {
            _remoteRom.Release.PublishDate = DateTime.UtcNow;
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.Bluray720p);
            _remoteRom.ParsedRomInfo.Languages = new List<Language> { Language.French };

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_quality_and_language_is_last_allowed_in_profile_and_bypass_enabled()
        {
            _delayProfile.UsenetDelay = 720;
            _delayProfile.BypassIfHighestQuality = true;

            _remoteRom.Release.PublishDate = DateTime.UtcNow;
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.Bluray720p);
            _remoteRom.ParsedRomInfo.Languages = new List<Language> { Language.French };

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_older_than_delay()
        {
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.HDTV720p);
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddHours(-10);

            _delayProfile.UsenetDelay = 60;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_release_is_younger_than_delay()
        {
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.SDTV);
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_release_is_a_proper_for_existing_episode()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p), Language.English);
            GivenUpgradeForExistingFile();

            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_release_is_a_real_for_existing_episode()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.PreferAndUpgrade);
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(real: 1));
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p), Language.English);
            GivenUpgradeForExistingFile();

            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotUpgrade);
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2));
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p), Language.English);
            GivenUpgradeForExistingFile();

            Mocker.GetMock<IUpgradableSpecification>()
                  .Setup(s => s.IsRevisionUpgrade(It.IsAny<QualityModel>(), It.IsAny<QualityModel>()))
                  .Returns(true);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_release_is_proper_for_existing_episode_of_different_quality()
        {
            _remoteRom.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL720p, new Revision(version: 2));
            _remoteRom.Release.PublishDate = DateTime.UtcNow;

            GivenExistingFile(new QualityModel(Quality.HDTV720p), Language.English);

            _delayProfile.UsenetDelay = 720;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_custom_format_score_is_above_minimum_but_bypass_disabled()
        {
            _remoteRom.Release.PublishDate = DateTime.UtcNow;
            _remoteRom.CustomFormatScore = 100;

            _delayProfile.UsenetDelay = 720;
            _delayProfile.MinimumCustomFormatScore = 50;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_custom_format_score_is_above_minimum_and_bypass_enabled_but_under_minimum()
        {
            _remoteRom.Release.PublishDate = DateTime.UtcNow;
            _remoteRom.CustomFormatScore = 5;

            _delayProfile.UsenetDelay = 720;
            _delayProfile.BypassIfAboveCustomFormatScore = true;
            _delayProfile.MinimumCustomFormatScore = 50;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_custom_format_score_is_above_minimum_and_bypass_enabled()
        {
            _remoteRom.Release.PublishDate = DateTime.UtcNow;
            _remoteRom.CustomFormatScore = 100;

            _delayProfile.UsenetDelay = 720;
            _delayProfile.BypassIfAboveCustomFormatScore = true;
            _delayProfile.MinimumCustomFormatScore = 50;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
