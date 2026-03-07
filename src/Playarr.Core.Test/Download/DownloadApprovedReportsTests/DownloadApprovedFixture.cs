using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Download.Clients;
using Playarr.Core.Download.Pending;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.Download.DownloadApprovedReportsTests
{
    [TestFixture]
    public class DownloadApprovedFixture : CoreTest<ProcessDownloadDecisions>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IPrioritizeDownloadDecision>()
                .Setup(v => v.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns<List<DownloadDecision>>(v => v);
        }

        private Rom GetEpisode(int id)
        {
            return Builder<Rom>.CreateNew()
                            .With(e => e.Id = id)
                            .With(e => e.EpisodeNumber = id)
                            .Build();
        }

        private RemoteEpisode GetRemoteEpisode(List<Rom> roms, QualityModel quality, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteRom = new RemoteEpisode();
            remoteRom.ParsedRomInfo = new ParsedRomInfo();
            remoteRom.ParsedRomInfo.Quality = quality;

            remoteRom.Roms = new List<Rom>();
            remoteRom.Roms.AddRange(roms);

            remoteRom.Release = new ReleaseInfo();
            remoteRom.Release.DownloadProtocol = downloadProtocol;
            remoteRom.Release.PublishDate = DateTime.UtcNow;

            remoteRom.Game = Builder<Game>.CreateNew()
                .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                .Build();

            return remoteRom;
        }

        [Test]
        public async Task should_download_report_if_episode_was_not_already_downloaded()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>(), null), Times.Once());
        }

        [Test]
        public async Task should_only_download_episode_once()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));
            decisions.Add(new DownloadDecision(remoteRom));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>(), null), Times.Once());
        }

        [Test]
        public async Task should_not_download_if_any_episode_was_already_downloaded()
        {
            var remoteRom1 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p));

            var remoteRom2 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(1), GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>(), null), Times.Once());
        }

        [Test]
        public async Task should_return_downloaded_reports()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));

            var result = await Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().HaveCount(1);
        }

        [Test]
        public async Task should_return_all_downloaded_reports()
        {
            var remoteRom1 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p));

            var remoteRom2 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));

            var result = await Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().HaveCount(2);
        }

        [Test]
        public async Task should_only_return_downloaded_reports()
        {
            var remoteRom1 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p));

            var remoteRom2 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p));

            var remoteRom3 = GetRemoteEpisode(
                                                    new List<Rom> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom1));
            decisions.Add(new DownloadDecision(remoteRom2));
            decisions.Add(new DownloadDecision(remoteRom3));

            var result = await Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().HaveCount(2);
        }

        [Test]
        public async Task should_not_add_to_downloaded_list_when_download_fails()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteEpisode>(), null)).Throws(new Exception());

            var result = await Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_an_empty_list_when_none_are_approved()
        {
            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(null, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!")));
            decisions.Add(new DownloadDecision(null, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!")));

            Subject.GetQualifiedReports(decisions).Should().BeEmpty();
        }

        [Test]
        public async Task should_not_grab_if_pending()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!", RejectionType.Temporary)));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>(), null), Times.Never());
        }

        [Test]
        public async Task should_not_add_to_pending_if_episode_was_grabbed()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));
            decisions.Add(new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!", RejectionType.Temporary)));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Never());
        }

        [Test]
        public async Task should_add_to_pending_even_if_already_added_to_pending()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!", RejectionType.Temporary)));
            decisions.Add(new DownloadDecision(remoteRom, new DownloadRejection(DownloadRejectionReason.Unknown, "Failure!", RejectionType.Temporary)));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Once());
        }

        [Test]
        public async Task should_add_to_failed_if_already_failed_for_that_protocol()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));
            decisions.Add(new DownloadDecision(remoteRom));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteEpisode>(), null))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>(), null), Times.Once());
        }

        [Test]
        public async Task should_not_add_to_failed_if_failed_for_a_different_protocol()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p), DownloadProtocol.Usenet);
            var remoteRom2 = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p), DownloadProtocol.Torrent);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));
            decisions.Add(new DownloadDecision(remoteRom2));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.Is<RemoteEpisode>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet), null))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            await Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteEpisode>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet), null), Times.Once());
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteEpisode>(r => r.Release.DownloadProtocol == DownloadProtocol.Torrent), null), Times.Once());
        }

        [Test]
        public async Task should_add_to_rejected_if_release_unavailable_on_indexer()
        {
            var roms = new List<Rom> { GetEpisode(1) };
            var remoteRom = GetRemoteEpisode(roms, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteRom));

            Mocker.GetMock<IDownloadService>()
                  .Setup(s => s.DownloadReport(It.IsAny<RemoteEpisode>(), null))
                  .Throws(new ReleaseUnavailableException(remoteRom.Release, "That 404 Error is not just a Quirk"));

            var result = await Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().BeEmpty();
            result.Rejected.Should().NotBeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
