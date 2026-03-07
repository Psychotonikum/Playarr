using System.Data;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(30)]
    public class add_season_folder_format_to_naming_config : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("PlatformFolderFormat").AsString().Nullable();
            Execute.WithConnection(ConvertConfig);
            Execute.Sql("DELETE FROM \"Config\" WHERE \"Key\" = 'seasonfolderformat'");
            Execute.Sql("DELETE FROM \"Config\" WHERE \"Key\" = 'useseasonfolder'");
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (var namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = "SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'seasonfolderformat'";
                var seasonFormat = "Platform {platform}";

                using (var namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    while (namingConfigReader.Read())
                    {
                        // only getting one column, so its index is 0
                        seasonFormat = namingConfigReader.GetString(0);

                        seasonFormat = seasonFormat.Replace("%sn", "{Game Title}")
                                                   .Replace("%s.n", "{Game.Title}")
                                                   .Replace("%s", "{platform}")
                                                   .Replace("%0s", "{platform:00}")
                                                   .Replace("%e", "{rom}")
                                                   .Replace("%0e", "{rom:00}");
                    }
                }

                using (var updateCmd = conn.CreateCommand())
                {
                    var text = string.Format("UPDATE \"NamingConfig\" " +
                                             "SET \"PlatformFolderFormat\" = '{0}'",
                                             seasonFormat);

                    updateCmd.Transaction = tran;
                    updateCmd.CommandText = text;
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
