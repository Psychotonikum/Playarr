using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Common.Crypto;
using Playarr.Core.Download.Pending;
using Playarr.Core.Lifecycle;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Rom _episode;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _episode = Builder<Rom>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllByGameId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(It.IsAny<int>()))
                  .Returns(new Game());

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetGame(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Game> { new Game() });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<Game>()))
                  .Returns(new RemoteRom { Roms = new List<Rom> { _episode } });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetRoms(It.IsAny<ParsedRomInfo>(), It.IsAny<Game>(), It.IsAny<bool>(), null))
                  .Returns(new List<Rom> { _episode });
        }

        private void AddPending(int id, int platformNumber, int[] roms)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 Title = "Game.Title.S01E05.abc-Playarr",
                 ParsedRomInfo = new ParsedRomInfo { PlatformNumber = platformNumber, RomNumbers = roms },
                 Release = Builder<ReleaseInfo>.CreateNew().Build()
             });
        }

        private void InitializeReleases()
        {
            Subject.Handle(new ApplicationStartedEvent());
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31($"pending-{1}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 2 });
            AddPending(id: 3, platformNumber: 2, roms: new[] { 3 });
            AddPending(id: 4, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31($"pending-{3}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_different_season()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 3, platformNumber: 3, roms: new[] { 1 });
            AddPending(id: 4, platformNumber: 3, roms: new[] { 1 });

            var queueId = HashConverter.GetHashInt31($"pending-{1}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_different_episodes()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 3, platformNumber: 2, roms: new[] { 2 });
            AddPending(id: 4, platformNumber: 2, roms: new[] { 3 });

            var queueId = HashConverter.GetHashInt31($"pending-{1}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multiepisodes()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31($"pending-{1}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singleepisodes()
        {
            AddPending(id: 1, platformNumber: 2, roms: new[] { 1 });
            AddPending(id: 2, platformNumber: 2, roms: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31($"pending-{2}");

            InitializeReleases();
            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(2);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }
}
