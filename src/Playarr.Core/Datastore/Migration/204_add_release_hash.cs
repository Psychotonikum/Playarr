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
    [Migration(204)]
    public class add_add_release_hash : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("ReleaseHash").AsString().Nullable();

            Execute.WithConnection(UpdateEpisodeFiles);
        }

        private void UpdateEpisodeFiles(IDbConnection conn, IDbTransaction tran)
        {
            var updates = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"SceneName\", \"RelativePath\", \"OriginalFilePath\" FROM \"EpisodeFiles\"";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var sceneName = reader[1] as string;
                    var relativePath = reader[2] as string;
                    var originalFilePath = reader[3] as string;

                    ParsedRomInfo parsedRomInfo = null;

                    var originalTitle = sceneName;

                    if (originalTitle.IsNullOrWhiteSpace() && originalFilePath.IsNotNullOrWhiteSpace())
                    {
                        originalTitle = Path.GetFileNameWithoutExtension(originalFilePath);
                    }

                    if (originalTitle.IsNotNullOrWhiteSpace())
                    {
                        parsedRomInfo = Parser.Parser.ParseTitle(originalTitle);
                    }

                    if (parsedRomInfo == null || parsedRomInfo.ReleaseHash.IsNullOrWhiteSpace())
                    {
                        parsedRomInfo = Parser.Parser.ParseTitle(Path.GetFileNameWithoutExtension(relativePath));
                    }

                    if (parsedRomInfo != null && parsedRomInfo.ReleaseHash.IsNotNullOrWhiteSpace())
                    {
                        updates.Add(new
                        {
                            Id = id,
                            ReleaseHash = parsedRomInfo.ReleaseHash
                        });
                    }
                }
            }

            if (updates.Count > 0)
            {
                var updateRomFilesSql = "UPDATE \"EpisodeFiles\" SET \"ReleaseHash\" = @ReleaseHash WHERE \"Id\" = @Id";
                conn.Execute(updateRomFilesSql, updates, transaction: tran);
            }
        }
    }
}
