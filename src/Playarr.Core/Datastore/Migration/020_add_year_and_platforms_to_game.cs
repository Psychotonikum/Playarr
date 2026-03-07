using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using Playarr.Common.Serializer;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(20)]
    public class add_year_and_seasons_to_series : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("Year").AsInt32().Nullable();
            Alter.Table("Series").AddColumn("Platforms").AsString().Nullable();

            Execute.WithConnection(ConvertSeasons);
        }

        private void ConvertSeasons(IDbConnection conn, IDbTransaction tran)
        {
            using (var allGamesCmd = conn.CreateCommand())
            {
                allGamesCmd.Transaction = tran;
                allGamesCmd.CommandText = "SELECT \"Id\" FROM \"Series\"";
                using (var allGamesReader = allGamesCmd.ExecuteReader())
                {
                    while (allGamesReader.Read())
                    {
                        var gameId = allGamesReader.GetInt32(0);
                        var platforms = new List<dynamic>();

                        using (var seasonsCmd = conn.CreateCommand())
                        {
                            seasonsCmd.Transaction = tran;
                            seasonsCmd.CommandText = $"SELECT \"SeasonNumber\", \"Monitored\" FROM \"Platforms\" WHERE \"SeriesId\" = {gameId}";

                            using (var seasonReader = seasonsCmd.ExecuteReader())
                            {
                                while (seasonReader.Read())
                                {
                                    var platformNumber = seasonReader.GetInt32(0);
                                    var monitored = seasonReader.GetBoolean(1);

                                    if (platformNumber == 0)
                                    {
                                        monitored = false;
                                    }

                                    platforms.Add(new { platformNumber, monitored });
                                }
                            }
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            var text = $"UPDATE \"Series\" SET \"Platforms\" = '{platforms.ToJson()}' WHERE \"Id\" = {gameId}";

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
