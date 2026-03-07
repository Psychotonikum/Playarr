using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedRomFilesFixture : DbTest<CleanupOrphanedRomFiles, RomFile>
    {
        [Test]
        public void should_delete_orphaned_episode_files()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(romFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_episode_files()
        {
            var romFiles = Builder<RomFile>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(romFiles);

            var rom = Builder<Rom>.CreateNew()
                .With(e => e.EpisodeFileId = romFiles.First().Id)
                .BuildNew();

            Db.Insert(rom);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            Db.All<Rom>().Should().Contain(e => e.EpisodeFileId == AllStoredModels.First().Id);
        }
    }
}
