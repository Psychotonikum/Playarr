using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore.Migration;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class metadata_files_extensionFixture : MigrationTest<extra_and_subtitle_files>
    {
        [Test]
        public void should_set_extension_using_relative_path()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    RelativePath = "banner.jpg",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Type = 3,
                    Consumer = "XbmcMetadata"
                });

                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "Game.Title.S01E01.jpg",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Type = 5,
                    Consumer = "XbmcMetadata"
                });

                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    RelativePath = "Game Title",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Type = 3,
                    Consumer = "RoksboxMetadata"
                });
            });

            var items = db.Query<MetadataFile99>("SELECT * FROM \"MetadataFiles\"");

            items.Should().HaveCount(2);
            items.First().Extension.Should().Be(".jpg");
            items.Last().Extension.Should().Be(".jpg");
        }
    }
}
