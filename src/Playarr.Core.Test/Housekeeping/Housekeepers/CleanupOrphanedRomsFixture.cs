using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedEpisodesFixture : DbTest<CleanupOrphanedEpisodes, Rom>
    {
        [Test]
        public void should_delete_orphaned_episodes()
        {
            var rom = Builder<Rom>.CreateNew()
                                          .BuildNew();

            Db.Insert(rom);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_episodes()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var roms = Builder<Rom>.CreateListOfSize(2)
                                          .TheFirst(1)
                                          .With(e => e.GameId = game.Id)
                                          .BuildListOfNew();

            Db.InsertMany(roms);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.GameId == game.Id);
        }
    }
}
