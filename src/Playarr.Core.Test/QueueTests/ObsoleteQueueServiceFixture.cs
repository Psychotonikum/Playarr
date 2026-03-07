using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Download;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Parser.Model;
using Playarr.Core.Queue;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.QueueTests
{
    [TestFixture]
    public class ObsoleteQueueServiceFixture : CoreTest<ObsoleteQueueService>
    {
        private List<TrackedDownload> _trackedDownloads;

        [SetUp]
        public void SetUp()
        {
            var downloadClientInfo = Builder<DownloadClientItemClientInfo>.CreateNew().Build();

            var downloadItem = Builder<Playarr.Core.Download.DownloadClientItem>.CreateNew()
                                        .With(v => v.RemainingTime = TimeSpan.FromSeconds(10))
                                        .With(v => v.DownloadClientInfo = downloadClientInfo)
                                        .Build();

            var game = Builder<Game>.CreateNew()
                                        .Build();

            var roms = Builder<Rom>.CreateListOfSize(3)
                                          .All()
                                          .With(e => e.SeriesId = game.Id)
                                          .Build();

            var remoteRom = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Game = game)
                                                   .With(r => r.Roms = new List<Rom>(roms))
                                                   .With(r => r.ParsedRomInfo = new ParsedRomInfo())
                                                   .Build();

            _trackedDownloads = Builder<TrackedDownload>.CreateListOfSize(1)
                .All()
                .With(v => v.IsTrackable = true)
                .With(v => v.DownloadItem = downloadItem)
                .With(v => v.RemoteEpisode = remoteRom)
                .Build()
                .ToList();
        }

        [Test]
        public void queue_items_should_have_id()
        {
            Subject.Handle(new TrackedDownloadRefreshedEvent(_trackedDownloads));

            var queue = Subject.GetQueue();

            queue.Should().HaveCount(3);

            queue.All(v => v.Id > 0).Should().BeTrue();

            var distinct = queue.Select(v => v.Id).Distinct().ToArray();

            distinct.Should().HaveCount(3);
        }
    }
}
