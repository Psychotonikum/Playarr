using System.Data;
using System.IO;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(57)]
    public class convert_episode_file_path_to_relative : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Column("RelativePath").OnTable("EpisodeFiles").AsString().Nullable();

            // TODO: Add unique constraint for game ID and Relative Path
            // TODO: Warn if multiple game share the same path

            Execute.WithConnection(UpdateRelativePaths);
        }

        private void UpdateRelativePaths(IDbConnection conn, IDbTransaction tran)
        {
            using (var getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = "SELECT \"Id\", \"Path\" FROM \"Series\"";
                using (var seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var gameId = seriesReader.GetInt32(0);
                        var gamePath = seriesReader.GetString(1) + Path.DirectorySeparatorChar;

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE \"EpisodeFiles\" SET \"RelativePath\" = REPLACE(\"Path\", ?, '') WHERE \"SeriesId\" = ?";
                            updateCmd.AddParameter(gamePath);
                            updateCmd.AddParameter(gameId);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
