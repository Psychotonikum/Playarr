using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class EpisodesRepositoryReadFixture : DbTest<RomRepository, Rom>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            Db.Insert(_series);
        }

        [Test]
        public void should_get_episodes_by_file()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(romFile);

            var rom = Builder<Rom>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.GameId = _series.Id)
                                        .With(e => e.EpisodeFileId = romFile.Id)
                                        .BuildListOfNew();

            Db.InsertMany(rom);

            var roms = Subject.GetEpisodeByFileId(romFile.Id);
            roms.Should().HaveCount(2);
        }
    }
}
