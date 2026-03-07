using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Download.Pending;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupDownloadClientUnavailablePendingReleasesFixture : DbTest<CleanupDownloadClientUnavailablePendingReleases, PendingRelease>
    {
        [Test]
        public void should_delete_old_DownloadClientUnavailable_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.Reason = PendingReleaseReason.DownloadClientUnavailable)
                .With(h => h.Added = DateTime.UtcNow.AddDays(-21))
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_old_Fallback_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.Reason = PendingReleaseReason.Fallback)
                .With(h => h.Added = DateTime.UtcNow.AddDays(-21))
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_old_Delay_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.Reason = PendingReleaseReason.Delay)
                .With(h => h.Added = DateTime.UtcNow.AddDays(-21))
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
