using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.MediaFiles.EpisodeImport.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private Game _series;
        private Rom _episode;
        private LocalEpisode _localRom;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.SeriesType = GameTypes.Standard)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            _episode = Builder<Rom>.CreateNew()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.AirDateUtc = DateTime.UtcNow)
                .Build();

            _localRom = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Roms = new List<Rom> { _episode },
                                    Game = _series
                                };

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                .Build();
        }

        private void GivenHistory(List<EpisodeHistory> history)
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByRomId(It.IsAny<int>()))
                .Returns(history);
        }

        [Test]
        public void should_accepted_if_download_client_item_is_null()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_does_not_have_file()
        {
            _episode.EpisodeFileId = 0;

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_has_not_been_imported()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(1)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_was_grabbed_after_being_imported()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(3)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .TheFirst(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow)
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_episode_imported_after_being_grabbed()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .TheFirst(1)
                .With(h => h.EventType = EpisodeHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localRom, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
