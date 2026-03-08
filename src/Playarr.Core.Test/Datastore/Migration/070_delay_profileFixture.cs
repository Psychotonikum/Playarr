using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore.Migration;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class delay_profileFixture : MigrationTest<delay_profile>
    {
        [Test]
        public void should_migrate_old_delays()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = 0,
                    Items = "[]"
                });

                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 2,
                    Name = "TwoHours",
                    Cutoff = 0,
                    Items = "[]"
                });
            });

            var allProfiles = db.Query<DelayProfile70>("SELECT * FROM \"DelayProfiles\"");

            allProfiles.Should().HaveCount(3);
            allProfiles.Should().OnlyContain(c => c.PreferredProtocol == 1);
            allProfiles.Should().OnlyContain(c => c.TorrentDelay == 0);
            allProfiles.Should().Contain(c => c.UsenetDelay == 60);
            allProfiles.Should().Contain(c => c.UsenetDelay == 120);
        }

        [Test]
        public void should_create_tag_for_delay_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = 0,
                    Items = "[]"
                });
            });

            var tags = db.Query<Tag69>("SELECT * FROM \"Tags\"");

            tags.Should().HaveCount(1);
            tags.First().Label.Should().Be("delay-60");
        }

        [Test]
        public void should_add_tag_to_series_that_had_a_profile_with_delay_attached()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                                                   {
                                                       GrabDelay = 1,
                                                       Name = "OneHour",
                                                       Cutoff = 0,
                                                       Items = "[]"
                                                   });

                c.Insert.IntoTable("Series").Row(new
                                                 {
                                                     TvdbId = 0,
                                                     MobyGamesId = 0,
                                                     Title = "Game",
                                                     TitleSlug = "game",
                                                     CleanTitle = "game",
                                                     Status = 0,
                                                     Images = "[]",
                                                     Path = @"C:\Test\Game",
                                                     Monitored = true,
                                                     PlatformFolder = true,
                                                     Runtime = 0,
                                                     SeriesType = 0,
                                                     UseSceneNumbering = false,
                                                     Tags = "[1]"
                                                 });
            });

            var tag = db.Query<Tag69>("SELECT \"Id\", \"Label\" FROM \"Tags\"").Single();
            var game = db.Query<Series69>("SELECT \"Tags\" FROM \"Series\"");

            game.Should().HaveCount(1);

            var actualTags = game.First().Tags;
            var expectedTags = Enumerable.Repeat(tag.Id, actualTags.Count);
            actualTags.Should().Equal(expectedTags);
        }
    }
}
