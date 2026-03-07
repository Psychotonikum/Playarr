using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Blocklisting;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistRepositoryFixture : DbTest<BlocklistRepository, Blocklist>
    {
        private Blocklist _blocklist;
        private Game _series1;
        private Game _series2;

        [SetUp]
        public void Setup()
        {
            _blocklist = new Blocklist
                     {
                         SeriesId = 12345,
                         RomIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.Bluray720p),
                         Languages = new List<Language> { Language.English },
                         SourceTitle = "game.title.s01e01",
                         Date = DateTime.UtcNow
                     };

            _series1 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 7)
                                      .Build();

            _series2 = Builder<Game>.CreateNew()
                                      .With(s => s.Id = 8)
                                      .Build();
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_blocklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_episode_ids()
        {
            Subject.Insert(_blocklist);

            Subject.All().First().RomIds.Should().Contain(_blocklist.RomIds);
        }

        [Test]
        public void should_check_for_blocklisted_title_case_insensative()
        {
            Subject.Insert(_blocklist);

            Subject.BlocklistedByTitle(_blocklist.SeriesId, _blocklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }

        [Test]
        public void should_delete_blocklists_by_gameId()
        {
            var blocklistItems = Builder<Blocklist>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.SeriesId = _series2.Id)
                .TheRest()
                .With(c => c.SeriesId = _series1.Id)
                .All()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Languages = new List<Language>())
                .With(c => c.RomIds = new List<int> { 1 })
                .BuildListOfNew();

            Db.InsertMany(blocklistItems);

            Subject.DeleteForGameIds(new List<int> { _series1.Id });

            var removedSeriesBlocklists = Subject.BlocklistedBySeries(_series1.Id);
            var nonRemovedSeriesBlocklists = Subject.BlocklistedBySeries(_series2.Id);

            removedSeriesBlocklists.Should().HaveCount(0);
            nonRemovedSeriesBlocklists.Should().HaveCount(1);
        }
    }
}
