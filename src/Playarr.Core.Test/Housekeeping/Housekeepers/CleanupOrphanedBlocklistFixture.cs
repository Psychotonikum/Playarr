using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Blocklisting;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlocklistFixture : DbTest<CleanupOrphanedBlocklist, Blocklist>
    {
        [Test]
        public void should_delete_orphaned_blocklist_items()
        {
            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.RomIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(blocklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blocklist_items()
        {
            var game = Builder<Game>.CreateNew().BuildNew();

            Db.Insert(game);

            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.RomIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .With(b => b.SeriesId = game.Id)
                .BuildNew();

            Db.Insert(blocklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
