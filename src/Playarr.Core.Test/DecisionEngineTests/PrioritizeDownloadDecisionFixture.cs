using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class PrioritizeDownloadDecisionFixture : CoreTest<DownloadDecisionPriorizationService>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            _series = Builder<Game>.CreateNew()
                .With(e => e.Runtime = 60)
                .With(e => e.QualityProfile = new QualityProfile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();
        }

        private void GivenPreferredSize(QualityProfile qualityProfile, double? size)
        {
            foreach (var qualityOrGroup in qualityProfile.Items)
            {
                if (qualityOrGroup.Quality != null)
                {
                    qualityOrGroup.PreferredSize = size;
                }
                else
                {
                    qualityOrGroup.Items.ForEach(i => i.PreferredSize = size);
                }
            }
        }

        private Rom GivenEpisode(int id)
        {
            return Builder<Rom>.CreateNew()
                            .With(e => e.Id = id)
                            .With(e => e.EpisodeNumber = id)
                            .Build();
        }

        private RemoteRom GivenRemoteEpisode(List<Rom> roms, QualityModel quality, Language language, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet, int indexerPriority = 25)
        {
            var remoteRom = new RemoteRom();
            remoteRom.ParsedRomInfo = new ParsedRomInfo();
            remoteRom.ParsedRomInfo.Quality = quality;
            remoteRom.ParsedRomInfo.Languages = new List<Language> { language };

            remoteRom.Roms = new List<Rom>();
            remoteRom.Roms.AddRange(roms);

            remoteRom.Release = new ReleaseInfo();
            remoteRom.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteRom.Release.Size = size;
            remoteRom.Release.DownloadProtocol = downloadProtocol;
            remoteRom.Release.IndexerPriority = indexerPriority;

            remoteRom.Game = _series;

            return remoteRom;
        }

        private void GivenPreferredDownloadProtocol(DownloadProtocol downloadProtocol)
        {
            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new DelayProfile
                  {
                      PreferredProtocol = downloadProtocol
                  });
        }

        [Test]
        public void should_put_reals_before_non_reals()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(version: 1, real: 0)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(version: 1, real: 1)), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Real.Should().Be(1);
        }

        [Test]
        public void should_put_propers_before_non_propers()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(version: 1)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(version: 2)), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Unknown), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Quality.Should().Be(Quality.Bad);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(2) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Roms.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes_with_multiple_episodes()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(2), GivenEpisode(3) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.Bad), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Roms.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteRomSd = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Unknown), Language.English, size: 100.Megabytes(), age: 1);
            var remoteRomHdSmallOld = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 1200.Megabytes(), age: 1000);
            var remoteRomSmallYoung = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 1250.Megabytes(), age: 10);
            var remoteRomHdLargeYoung = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRomSd));
            decisions.Add(new DownloadDecision(remoteRomHdSmallOld));
            decisions.Add(new DownloadDecision(remoteRomSmallYoung));
            decisions.Add(new DownloadDecision(remoteRomHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRomHdLargeYoung);
        }

        [Test]
        public void should_order_by_closest_to_preferred_size_if_both_under()
        {
            // 200 MB/Min * 60 Min Runtime = 12000 MB
            GivenPreferredSize(_series.QualityProfile.Value, 200);

            var remoteRomSmall = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 1200.Megabytes(), age: 1);
            var remoteRomLarge = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 10000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRomSmall));
            decisions.Add(new DownloadDecision(remoteRomLarge));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRomLarge);
        }

        [Test]
        public void should_order_by_closest_to_preferred_size_if_preferred_is_in_between()
        {
            // 46 MB/Min * 60 Min Runtime = 6900 MB
            GivenPreferredSize(_series.QualityProfile.Value, 46);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 500.Megabytes(), age: 1);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 2000.Megabytes(), age: 1);
            var remoteRom3 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 3000.Megabytes(), age: 1);
            var remoteRom4 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 5000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));
            decisions.Add(new DownloadDecision(remoteRom3));
            decisions.Add(new DownloadDecision(remoteRom4));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRom3);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, age: 10);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, age: 5);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRom2);
        }

        [Test]
        public void should_not_throw_if_no_episodes_are_found()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 500.Megabytes());
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, size: 500.Megabytes());

            remoteRom1.Roms = new List<Rom>();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            Subject.PrioritizeDecisions(decisions);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, downloadProtocol: DownloadProtocol.Torrent);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, downloadProtocol: DownloadProtocol.Torrent);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English, downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_season_pack_above_single_episode()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            remoteRom1.ParsedRomInfo.FullSeason = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.FullSeason.Should().BeTrue();
        }

        [Test]
        public void should_prefer_single_episode_over_multi_episode_for_non_anime()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Roms.Count.Should().Be(remoteRom2.Roms.Count);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteRom1.Release = torrentInfo1;
            remoteRom2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteRom.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteRom1.Release = torrentInfo1;
            remoteRom2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteRom.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteRom1.Release = torrentInfo1;
            remoteRom2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteRom.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 1000;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteRom1.Release = torrentInfo1;
            remoteRom2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteRom.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            remoteRom1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteRom1.Release.Size = 200.Megabytes();

            remoteRom2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteRom2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Release.Should().Be(remoteRom1.Release);
        }

        [Test]
        public void should_prefer_quality_over_the_number_of_peers()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Verified), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Unknown), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 100;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteRom1.Release = torrentInfo1;
            remoteRom2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteRom.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_put_higher_quality_before_lower_always()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Unknown), Language.French);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.German);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Quality.Should().Be(Quality.Bad);
        }

        [Test]
        public void should_prefer_higher_score_over_lower_score()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad), Language.English);

            remoteRom1.CustomFormatScore = 10;
            remoteRom2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.CustomFormatScore.Should().Be(10);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_prefer_and_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(2)), Language.English);

            remoteRom1.CustomFormatScore = 10;
            remoteRom2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_proper_over_score_when_download_propers_is_do_not_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(2)), Language.English);

            remoteRom1.CustomFormatScore = 10;
            remoteRom2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_prefer_score_over_proper_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(2)), Language.English);

            remoteRom1.CustomFormatScore = 10;
            remoteRom2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Quality.Should().Be(Quality.Bad);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteRom.CustomFormatScore.Should().Be(10);
        }

        [Test]
        public void should_prefer_score_over_real_when_download_propers_is_do_not_prefer()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1, 0)), Language.English);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1, 1)), Language.English);

            remoteRom1.CustomFormatScore = 10;
            remoteRom2.CustomFormatScore = 0;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Quality.Should().Be(Quality.Bad);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Version.Should().Be(1);
            qualifiedReports.First().RemoteRom.ParsedRomInfo.Quality.Revision.Real.Should().Be(0);
            qualifiedReports.First().RemoteRom.CustomFormatScore.Should().Be(10);
        }

        [Test]
        public void sort_download_decisions_based_on_indexer_priority()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 25);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 50);
            var remoteRom3 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 1);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteRom1), new DownloadDecision(remoteRom2), new DownloadDecision(remoteRom3) });

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRom3);
            qualifiedReports.Skip(1).First().RemoteRom.Should().Be(remoteRom1);
            qualifiedReports.Last().RemoteRom.Should().Be(remoteRom2);
        }

        [Test]
        public void ensure_download_decisions_indexer_priority_is_not_perfered_over_quality()
        {
            var remoteRom1 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 25);
            var remoteRom2 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 50);
            var remoteRom3 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Unknown, new Revision(1)), Language.English, indexerPriority: 1);
            var remoteRom4 = GivenRemoteEpisode(new List<Rom> { GivenEpisode(1) }, new QualityModel(Quality.Bad, new Revision(1)), Language.English, indexerPriority: 25);

            var decisions = new List<DownloadDecision>();
            decisions.AddRange(new[] { new DownloadDecision(remoteRom1), new DownloadDecision(remoteRom2), new DownloadDecision(remoteRom3), new DownloadDecision(remoteRom4) });

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteRom.Should().Be(remoteRom1);
            qualifiedReports.Skip(1).First().RemoteRom.Should().Be(remoteRom4);
            qualifiedReports.Skip(2).First().RemoteRom.Should().Be(remoteRom2);
            qualifiedReports.Last().RemoteRom.Should().Be(remoteRom3);
        }
    }
}
