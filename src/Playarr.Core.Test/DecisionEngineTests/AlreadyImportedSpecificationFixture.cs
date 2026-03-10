using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Configuration;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.History;
using Playarr.Core.Indexers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const string TITLE = "Game.Title.S01E01.720p.HDTV.x264-Playarr";

        private Game _series;
        private QualityModel _hdtv720p;
        private QualityModel _hdtv1080p;
        private RemoteRom _remoteRom;
        private List<EpisodeHistory> _history;

        [SetUp]
        public void Setup()
        {
            var singleEpisodeList = new List<Rom>
                                    {
                                        new Rom
                                        {
                                            Id = FIRST_EPISODE_ID,
                                            PlatformNumber = 12,
                                            EpisodeNumber = 3,
                                            EpisodeFileId = 1
                                        }
                                    };

            _series = Builder<Game>.CreateNew()
                                     .Build();

            _hdtv720p = new QualityModel(Quality.HDTV720p, new Revision(version: 1));
            _hdtv1080p = new QualityModel(Quality.HDTV1080p, new Revision(version: 1));

            _remoteRom = new RemoteRom
            {
                Game = _series,
                ParsedRomInfo = new ParsedRomInfo { Quality = _hdtv720p },
                Roms = singleEpisodeList,
                Release = Builder<ReleaseInfo>.CreateNew()
                                              .Build()
            };

            _history = new List<EpisodeHistory>();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByRomId(It.IsAny<int>()))
                  .Returns(_history);
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        private void GivenHistoryItem(string downloadId, string sourceTitle, QualityModel quality, EpisodeHistoryEventType eventType)
        {
            _history.Add(new EpisodeHistory
                         {
                             DownloadId = downloadId,
                             SourceTitle = sourceTitle,
                             Quality = quality,
                             Date = DateTime.UtcNow,
                             EventType = eventType
                         });
        }

        [Test]
        public void should_be_accepted_if_CDH_is_disabled()
        {
            GivenCdhDisabled();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_a_file()
        {
            _remoteRom.Roms.First().EpisodeFileId = 0;

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_grabbed_event()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_imported_event()
        {
            GivenHistoryItem(Guid.NewGuid().ToString().ToUpper(), TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_and_imported_quality_is_the_same()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.DownloadFolderImported);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_id_and_release_torrent_hash_is_unknown()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteRom.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = null)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_does_not_have_an_id()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(null, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteRom.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_grabbed_download_id_matches_release_torrent_hash()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteRom.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_release_title_matches_grabbed_event_source_title()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteRom.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
