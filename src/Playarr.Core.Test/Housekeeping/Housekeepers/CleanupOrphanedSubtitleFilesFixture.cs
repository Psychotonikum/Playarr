using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Extras.Subtitles;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedSubtitleFilesFixture : DbTest<CleanupOrphanedSubtitleFiles, SubtitleFile>
    {
        [Test]
        public void should_delete_subtitle_files_that_dont_have_a_coresponding_series()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(romFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.EpisodeFileId = romFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            Db.Insert(subtitleFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_subtitle_files_that_have_a_coresponding_series()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(game);
            Db.Insert(romFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.EpisodeFileId = romFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            Db.Insert(subtitleFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_subtitle_files_that_dont_have_a_coresponding_episode_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.EpisodeFileId = 10)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            Db.Insert(subtitleFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_subtitle_files_that_have_a_coresponding_episode_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(game);
            Db.Insert(romFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.GameId = game.Id)
                                                    .With(m => m.EpisodeFileId = romFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            Db.Insert(subtitleFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_subtitle_files_that_have_episodefileid_of_zero()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                 .With(m => m.GameId = game.Id)
                                                 .With(m => m.EpisodeFileId = 0)
                                                 .With(m => m.Language = Language.English)
                                                 .BuildNew();

            Db.Insert(subtitleFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }
    }
}
