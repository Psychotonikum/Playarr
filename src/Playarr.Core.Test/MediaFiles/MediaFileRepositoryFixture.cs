using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, RomFile>
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
        public void get_files_by_series()
        {
            var files = Builder<RomFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel(Quality.Bluray720p))
                .Random(4)
                .With(s => s.SeriesId = 12)
                .BuildListOfNew();

            Db.InsertMany(files);

            var seriesFiles = Subject.GetFilesBySeries(12);

            seriesFiles.Should().HaveCount(4);
            seriesFiles.Should().OnlyContain(c => c.SeriesId == 12);
        }

        [Test]
        public void should_delete_files_by_gameId()
        {
            var items = Builder<RomFile>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.SeriesId = _series2.Id)
                .TheRest()
                .With(c => c.SeriesId = _series1.Id)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .BuildListOfNew();

            Db.InsertMany(items);

            Subject.DeleteForSeries(new List<int> { _series1.Id });

            var removedItems = Subject.GetFilesBySeries(_series1.Id);
            var nonRemovedItems = Subject.GetFilesBySeries(_series2.Id);

            removedItems.Should().HaveCount(0);
            nonRemovedItems.Should().HaveCount(1);
        }
    }
}
