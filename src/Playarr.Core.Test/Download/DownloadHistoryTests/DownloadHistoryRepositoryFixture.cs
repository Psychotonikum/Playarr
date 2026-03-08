using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Download.History;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Download.DownloadHistoryTests
{
    [TestFixture]
    public class DownloadHistoryRepositoryFixture : DbTest<DownloadHistoryRepository, DownloadHistory>
    {
        private Game _series1;
        private Game _series2;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 7)
                                      .Build();

            _series2 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 8)
                                      .Build();
        }

        [Test]
        public void should_delete_history_items_by_gameId()
        {
            var items = Builder<DownloadHistory>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.Id = 0)
                .With(c => c.GameId = _series2.Id)
                .TheRest()
                .With(c => c.Id = 0)
                .With(c => c.GameId = _series1.Id)
                .BuildListOfNew();

            Db.InsertMany(items);

            Subject.DeleteByGameIds(new List<int> { _series1.Id });

            var removedItems = Subject.All().Where(h => h.GameId == _series1.Id);
            var nonRemovedItems = Subject.All().Where(h => h.GameId == _series2.Id);

            removedItems.Should().HaveCount(0);
            nonRemovedItems.Should().HaveCount(1);
        }
    }
}
