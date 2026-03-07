using System.Collections.Generic;
using System.Linq;
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
    public class EpisodesWithFilesFixture : DbTest<RomRepository, Rom>
    {
        private const int SERIES_ID = 1;
        private List<Rom> _episodes;
        private List<RomFile> _romFiles;

        [SetUp]
        public void Setup()
        {
            _romFiles = Builder<RomFile>.CreateListOfSize(5)
                                                .All()
                                                .With(c => c.Quality = new QualityModel())
                                                .With(c => c.Languages = new List<Language> { Language.English })
                                                .BuildListOfNew();

            Db.InsertMany(_romFiles);

            _episodes = Builder<Rom>.CreateListOfSize(10)
                                        .All()
                                        .With(e => e.EpisodeFileId = 0)
                                        .With(e => e.SeriesId = SERIES_ID)
                                        .BuildListOfNew()
                                        .ToList();

            for (var i = 0; i < _romFiles.Count; i++)
            {
                _episodes[i].EpisodeFileId = _romFiles[i].Id;
            }

            Db.InsertMany(_episodes);
        }

        [Test]
        public void should_only_get_files_that_have_episode_files()
        {
            var result = Subject.EpisodesWithFiles(SERIES_ID);

            result.Should().OnlyContain(e => e.EpisodeFileId > 0);
            result.Should().HaveCount(_romFiles.Count);
        }

        [Test]
        public void should_only_contain_episodes_for_the_given_series()
        {
            var romFile = Builder<RomFile>.CreateNew()
                                                  .With(f => f.RelativePath = "another path")
                                                  .With(c => c.Quality = new QualityModel())
                                                  .With(c => c.Languages = new List<Language> { Language.English })
                                                  .BuildNew();

            Db.Insert(romFile);

            var rom = Builder<Rom>.CreateNew()
                                          .With(e => e.SeriesId = SERIES_ID + 10)
                                          .With(e => e.EpisodeFileId = romFile.Id)
                                          .BuildNew();

            Db.Insert(rom);

            Subject.EpisodesWithFiles(rom.SeriesId).Should().OnlyContain(e => e.SeriesId == rom.SeriesId);
        }

        [Test]
        public void should_have_episode_file_loaded()
        {
            Subject.EpisodesWithFiles(SERIES_ID).Should().OnlyContain(e => e.RomFile.IsLoaded);
        }
    }
}
