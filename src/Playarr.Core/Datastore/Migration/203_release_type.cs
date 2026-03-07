using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using FluentMigrator;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(203)]
    public class release_type : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blocklist").AddColumn("ReleaseType").AsInt32().WithDefaultValue(0);
            Alter.Table("EpisodeFiles").AddColumn("ReleaseType").AsInt32().WithDefaultValue(0);

            Execute.WithConnection(UpdateEpisodeFiles);
        }

        private void UpdateEpisodeFiles(IDbConnection conn, IDbTransaction tran)
        {
            var updates = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"OriginalFilePath\" FROM \"EpisodeFiles\" WHERE \"OriginalFilePath\" IS NOT NULL";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var originalFilePath = reader.GetString(1);

                    var folderName = Path.GetDirectoryName(originalFilePath);
                    var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                    var title = folderName.IsNullOrWhiteSpace() ? fileName : folderName;
                    var parsedRomInfo = Parser.Parser.ParseTitle(title);

                    if (parsedRomInfo != null && parsedRomInfo.ReleaseType != ReleaseType.Unknown)
                    {
                        updates.Add(new
                        {
                            Id = id,
                            ReleaseType = (int)parsedRomInfo.ReleaseType
                        });
                    }
                }
            }

            var updateRomFilesSql = "UPDATE \"EpisodeFiles\" SET \"ReleaseType\" = @ReleaseType WHERE \"Id\" = @Id";
            conn.Execute(updateRomFilesSql, updates, transaction: tran);
        }
    }
}
