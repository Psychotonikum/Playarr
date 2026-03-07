using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Extras.Metadata;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.Housekeeping.Housekeepers;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMetadataFilesFixture : DbTest<CleanupOrphanedMetadataFiles, MetadataFile>
    {
        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_series()
        {
            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.EpisodeFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_series()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.SeriesId = game.Id)
                                                    .With(m => m.EpisodeFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_episode_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.SeriesId = game.Id)
                                                    .With(m => m.EpisodeFileId = 10)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_episode_file()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            var romFile = Builder<RomFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(game);
            Db.Insert(romFile);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.SeriesId = game.Id)
                                                    .With(m => m.EpisodeFileId = romFile.Id)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_episode_metadata_files_that_have_episodefileid_of_zero()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                 .With(m => m.SeriesId = game.Id)
                                                 .With(m => m.Type = MetadataType.EpisodeMetadata)
                                                 .With(m => m.EpisodeFileId = 0)
                                                 .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_episode_image_files_that_have_episodefileid_of_zero()
        {
            var game = Builder<Game>.CreateNew()
                                        .BuildNew();

            Db.Insert(game);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.SeriesId = game.Id)
                                                    .With(m => m.Type = MetadataType.EpisodeImage)
                                                    .With(m => m.EpisodeFileId = 0)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }
    }
}
