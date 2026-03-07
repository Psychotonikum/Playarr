using System.Data;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(166)]
    public class update_series_sort_title : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateSortTitles);
        }

        private void UpdateSortTitles(IDbConnection conn, IDbTransaction tran)
        {
            using (var getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = "SELECT \"Id\", \"TvdbId\", \"Title\" FROM \"Series\"";
                using (var seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var igdbId = seriesReader.GetInt32(1);
                        var title = seriesReader.GetString(2);

                        var sortTitle = GameTitleNormalizer.Normalize(title, igdbId);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE \"Series\" SET \"SortTitle\" = ? WHERE \"Id\" = ?";
                            updateCmd.AddParameter(sortTitle);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
