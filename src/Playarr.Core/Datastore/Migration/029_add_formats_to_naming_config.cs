using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(29)]
    public class add_formats_to_naming_config : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("StandardEpisodeFormat").AsString().Nullable();
            Alter.Table("NamingConfig").AddColumn("DailyEpisodeFormat").AsString().Nullable();

            Execute.WithConnection(ConvertConfig);
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (var namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = "SELECT * FROM \"NamingConfig\" LIMIT 1";
                using (var namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    var separatorIndex = namingConfigReader.GetOrdinal("Separator");
                    var numberStyleIndex = namingConfigReader.GetOrdinal("NumberStyle");
                    var includeGameTitleIndex = namingConfigReader.GetOrdinal("IncludeGameTitle");
                    var includeRomTitleIndex = namingConfigReader.GetOrdinal("IncludeRomTitle");
                    var includeQualityIndex = namingConfigReader.GetOrdinal("IncludeQuality");
                    var replaceSpacesIndex = namingConfigReader.GetOrdinal("ReplaceSpaces");

                    while (namingConfigReader.Read())
                    {
                        var separator = namingConfigReader.GetString(separatorIndex);
                        var numberStyle = namingConfigReader.GetInt32(numberStyleIndex);
                        var includeGameTitle = namingConfigReader.GetBoolean(includeGameTitleIndex);
                        var includeRomTitle = namingConfigReader.GetBoolean(includeRomTitleIndex);
                        var includeQuality = namingConfigReader.GetBoolean(includeQualityIndex);
                        var replaceSpaces = namingConfigReader.GetBoolean(replaceSpacesIndex);

                        // Output settings
                        var gameTitlePattern = "";
                        var romTitlePattern = "";
                        var dailyEpisodePattern = "{Air-Date}";
                        var qualityFormat = " [{Quality Title}]";

                        if (includeGameTitle)
                        {
                            if (replaceSpaces)
                            {
                                gameTitlePattern = "{Game.Title}";
                            }
                            else
                            {
                                gameTitlePattern = "{Game Title}";
                            }

                            gameTitlePattern += separator;
                        }

                        if (includeRomTitle)
                        {
                            romTitlePattern = separator;

                            if (replaceSpaces)
                            {
                                romTitlePattern += "{Rom.Title}";
                            }
                            else
                            {
                                romTitlePattern += "{Rom Title}";
                            }
                        }

                        var standardEpisodeFormat = string.Format("{0}{1}{2}",
                            gameTitlePattern,
                            GetNumberStyle(numberStyle).Pattern,
                            romTitlePattern);

                        var dailyEpisodeFormat = string.Format("{0}{1}{2}",
                            gameTitlePattern,
                            dailyEpisodePattern,
                            romTitlePattern);

                        if (includeQuality)
                        {
                            if (replaceSpaces)
                            {
                                qualityFormat = ".[{Quality.Title}]";
                            }

                            standardEpisodeFormat += qualityFormat;
                            dailyEpisodeFormat += qualityFormat;
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE \"NamingConfig\" " +
                                                     "SET \"StandardEpisodeFormat\" = '{0}', " +
                                                     "\"DailyEpisodeFormat\" = '{1}'",
                                                     standardEpisodeFormat,
                                                     dailyEpisodeFormat);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static readonly List<dynamic> NumberStyles = new List<dynamic>
                                                                            {
                                                                                new
                                                                                    {
                                                                                        Id = 0,
                                                                                        Name = "1x05",
                                                                                        Pattern = "{platform}x{rom:00}",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 1,
                                                                                        Name = "01x05",
                                                                                        Pattern = "{platform:00}x{rom:00}",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 2,
                                                                                        Name = "S01E05",
                                                                                        Pattern = "S{platform:00}E{rom:00}",
                                                                                        EpisodeSeparator = "E"
                                                                                    },
                                                                                new
                                                                                    {
                                                                                        Id = 3,
                                                                                        Name = "s01e05",
                                                                                        Pattern = "s{platform:00}e{rom:00}",
                                                                                        EpisodeSeparator = "e"
                                                                                    }
                                                                            };

        private static dynamic GetNumberStyle(int id)
        {
            return NumberStyles.Single(s => s.Id == id);
        }
    }
}
