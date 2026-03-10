using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.History;
using Playarr.Core.Indexers;
using Playarr.Core.Indexers.TorrentRss;
using Playarr.Core.Languages;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                 new EpisodeHistory()
                {
                     DownloadId = "35238",
                     SourceTitle = "TV Game S01",
                     GameId = 5,
                     EpisodeId = 4
                }
                });
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteRom = new RemoteRom
            {
                Game = new Game() { Id = 5 },
                Roms = new List<Rom> { new Rom { Id = 4 } },
                ParsedRomInfo = new ParsedRomInfo()
                {
                    GameTitle = "TV Game",
                    PlatformNumber = 1
                },
                MappedPlatformNumber = 1
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedRomInfo>(i => i.PlatformNumber == 1 && i.GameTitle == "TV Game"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteRom);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteRom.Should().NotBeNull();
            trackedDownload.RemoteRom.Game.Should().NotBeNull();
            trackedDownload.RemoteRom.Game.Id.Should().Be(5);
            trackedDownload.RemoteRom.Roms.First().Id.Should().Be(4);
            trackedDownload.RemoteRom.ParsedRomInfo.PlatformNumber.Should().Be(1);
            trackedDownload.RemoteRom.MappedPlatformNumber.Should().Be(1);
        }

        [Test]
        public void should_set_indexer()
        {
            var episodeHistory = new EpisodeHistory()
            {
                DownloadId = "35238",
                SourceTitle = "TV Game S01",
                GameId = 5,
                EpisodeId = 4,
                EventType = EpisodeHistoryEventType.Grabbed,
            };
            episodeHistory.Data.Add("indexer", "MyIndexer (Prowlarr)");
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                    episodeHistory
                });

            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer (Prowlarr)",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(indexerDefinition.Id))
                .Returns(indexerDefinition);
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition });

            var remoteRom = new RemoteRom
            {
                Game = new Game() { Id = 5 },
                Roms = new List<Rom> { new Rom { Id = 4 } },
                ParsedRomInfo = new ParsedRomInfo()
                {
                    GameTitle = "TV Game",
                    PlatformNumber = 1
                },
                MappedPlatformNumber = 1
            };

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .Returns(remoteRom);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV.Game.S01.MULTi.1080p.WEB.H265-RlsGroup",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteRom.Should().NotBeNull();
            trackedDownload.RemoteRom.Release.Should().NotBeNull();
            trackedDownload.RemoteRom.Release.Indexer.Should().Be("MyIndexer (Prowlarr)");
        }

        [Test]
        public void should_parse_as_special_when_source_title_parsing_fails()
        {
            var remoteRom = new RemoteRom
            {
                Game = new Game() { Id = 5 },
                Roms = new List<Rom> { new Rom { Id = 4 } },
                ParsedRomInfo = new ParsedRomInfo()
                {
                    GameTitle = "TV Game",
                    PlatformNumber = 0,
                    RomNumbers = new[] { 1 }
                },
                MappedPlatformNumber = 0
            };

            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                 new EpisodeHistory()
                {
                     DownloadId = "35238",
                     SourceTitle = "TV Game Special",
                     GameId = 5,
                     EpisodeId = 4
                }
                });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedRomInfo>(i => i.PlatformNumber == 0 && i.GameTitle == "TV Game"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteRom);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.ParseSpecialRomTitle(It.IsAny<ParsedRomInfo>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(remoteRom.ParsedRomInfo);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteRom.Should().NotBeNull();
            trackedDownload.RemoteRom.Game.Should().NotBeNull();
            trackedDownload.RemoteRom.Game.Id.Should().Be(5);
            trackedDownload.RemoteRom.Roms.First().Id.Should().Be(4);
            trackedDownload.RemoteRom.ParsedRomInfo.PlatformNumber.Should().Be(0);
            trackedDownload.RemoteRom.MappedPlatformNumber.Should().Be(0);
        }

        [Test]
        public void should_unmap_tracked_download_if_episode_deleted()
        {
            GivenDownloadHistory();

            var remoteRom = new RemoteRom
                                {
                                    Game = new Game() { Id = 5 },
                                    Roms = new List<Rom> { new Rom { Id = 4 } },
                                    ParsedRomInfo = new ParsedRomInfo()
                                                        {
                                                            GameTitle = "TV Game",
                                                            PlatformNumber = 1,
                                                            RomNumbers = new[] { 1 }
                                                        },
                                    MappedPlatformNumber = 0
                                };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(remoteRom);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Game - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                                     {
                                         Id = 1,
                                         Type = "Blackhole",
                                         Name = "Blackhole Client",
                                         Protocol = DownloadProtocol.Torrent
                                     }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteRom));

            Subject.Handle(new RomInfoRefreshedEvent(remoteRom.Game, new List<Rom>(), new List<Rom>(), remoteRom.Roms));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteRom.Should().BeNull();
        }

        [Test]
        public void should_not_throw_when_processing_deleted_episodes()
        {
            GivenDownloadHistory();

            var remoteRom = new RemoteRom
            {
                Game = new Game() { Id = 5 },
                Roms = new List<Rom> { new Rom { Id = 4 } },
                ParsedRomInfo = new ParsedRomInfo()
                {
                    GameTitle = "TV Game",
                    PlatformNumber = 1,
                    RomNumbers = new[] { 1 }
                },
                MappedPlatformNumber = 0
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteRom));

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Game - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteRom));

            Subject.Handle(new RomInfoRefreshedEvent(remoteRom.Game, new List<Rom>(), new List<Rom>(), remoteRom.Roms));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteRom.Should().BeNull();
        }

        [Test]
        public void should_not_throw_when_processing_deleted_series()
        {
            GivenDownloadHistory();

            var remoteRom = new RemoteRom
            {
                Game = new Game() { Id = 5 },
                Roms = new List<Rom> { new Rom { Id = 4 } },
                ParsedRomInfo = new ParsedRomInfo()
                {
                    GameTitle = "TV Game",
                    PlatformNumber = 1,
                    RomNumbers = new[] { 1 }
                },
                MappedPlatformNumber = 0
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteRom));

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Game - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteRom));

            Subject.Handle(new SeriesDeletedEvent(new List<Game> { remoteRom.Game }, true, true));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteRom.Should().BeNull();
        }
    }
}
