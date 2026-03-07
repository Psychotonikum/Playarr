using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Datastore.Migration;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class force_lib_updateFixture : MigrationTest<force_lib_update>
    {
        [Test]
        public void should_not_fail_on_empty_db()
        {
            var db = WithMigrationTestDb();

            db.Query("SELECT * FROM \"ScheduledTasks\"").Should().BeEmpty();
            db.Query("SELECT * FROM \"Game\"").Should().BeEmpty();
        }

        [Test]
        public void should_reset_job_last_execution_time()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ScheduledTasks").Row(new
                {
                    TypeName = "Playarr.Core.Games.Commands.RefreshSeriesCommand",
                    Interval = 10,
                    LastExecution = "2000-01-01 00:00:00"
                });

                c.Insert.IntoTable("ScheduledTasks").Row(new
                {
                    TypeName = "Playarr.Core.Backup.BackupCommand",
                    Interval = 10,
                    LastExecution = "2000-01-01 00:00:00"
                });
            });

            var jobs = db.Query<ScheduledTasks75>("SELECT \"TypeName\", \"LastExecution\" FROM \"ScheduledTasks\"");

            jobs.Single(c => c.TypeName == "Playarr.Core.Games.Commands.RefreshSeriesCommand")
                .LastExecution.Year.Should()
                .Be(2014);

            jobs.Single(c => c.TypeName == "Playarr.Core.Backup.BackupCommand")
               .LastExecution.Year.Should()
               .Be(2000);
        }

        [Test]
        public void should_reset_series_last_sync_time()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "Profile1",
                    Cutoff = 0,
                    Items = "[]",
                    Language = 1
                });

                c.Insert.IntoTable("Game").Row(new
                {
                    TvdbId = 1,
                    MobyGamesId =1,
                    Title ="Title1",
                    CleanTitle ="CleanTitle1",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = true,
                    PlatformFolder = true,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = false,
                    LastInfoSync = "2000-01-01 00:00:00",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Game").Row(new
                {
                    TvdbId = 2,
                    MobyGamesId = 2,
                    Title = "Title2",
                    CleanTitle = "CleanTitle2",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test2",
                    Monitored = true,
                    PlatformFolder = true,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = false,
                    LastInfoSync = "2000-01-01 00:00:00",
                    ProfileId = 1
                });
            });

            var game = db.Query<Series69>("SELECT \"LastInfoSync\" FROM \"Game\"");

            game.Should().OnlyContain(c => c.LastInfoSync.Value.Year == 2014);
        }
    }
}
