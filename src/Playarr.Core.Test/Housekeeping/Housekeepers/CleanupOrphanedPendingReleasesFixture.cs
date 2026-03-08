using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Download.Pending;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedPendingReleasesFixture : DbTest<CleanupOrphanedPendingReleases, PendingRelease>
    {
        [Test]
        public void should_delete_orphaned_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_pending_items()
        {
            var game = Builder<Game>.CreateNew().BuildNew();

            Db.Insert(game);

            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.GameId = game.Id)
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
