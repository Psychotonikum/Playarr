using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore.Converters;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Languages;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(102)]
    public class add_language_to_romFiles_history_and_blacklist : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Alter.Table("History")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Alter.Table("Blacklist")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Execute.WithConnection(UpdateLanguage);
        }

        private void UpdateLanguage(IDbConnection conn, IDbTransaction tran)
        {
            var languageConverter = new EmbeddedDocumentConverter<List<Language>>(new LanguageIntConverter());

            var profileLanguages = new Dictionary<int, int>();
            using (var getProfileCmd = conn.CreateCommand())
            {
                getProfileCmd.Transaction = tran;
                getProfileCmd.CommandText = "SELECT \"Id\", \"Language\" FROM \"Profiles\"";

                using (var profilesReader = getProfileCmd.ExecuteReader())
                {
                    while (profilesReader.Read())
                    {
                        var profileId = profilesReader.GetInt32(0);
                        var episodeLanguage = Language.English.Id;
                        try
                        {
                            episodeLanguage = profilesReader.GetInt32(1);
                        }
                        catch (InvalidCastException e)
                        {
                            _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                        }

                        profileLanguages[profileId] = episodeLanguage;
                    }
                }
            }

            var seriesLanguages = new Dictionary<int, int>();
            using (var getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = "SELECT \"Id\", \"ProfileId\" FROM \"Series\"";
                using (var seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var gameId = seriesReader.GetInt32(0);
                        var seriesProfileId = seriesReader.GetInt32(1);

                        seriesLanguages[gameId] = profileLanguages.GetValueOrDefault(seriesProfileId, Language.English.Id);
                    }
                }
            }

            foreach (var group in seriesLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var language = new List<Language> { Language.FindById(group.Key) };

                var gameIds = group.Select(v => v.ToString()).Join(",");

                using (var updateRomFilesCmd = conn.CreateCommand())
                {
                    updateRomFilesCmd.Transaction = tran;
                    updateRomFilesCmd.CommandText = $"UPDATE \"EpisodeFiles\" SET \"Language\" = ? WHERE \"SeriesId\" IN ({gameIds})";
                    var param = updateRomFilesCmd.CreateParameter();
                    languageConverter.SetValue(param, language);
                    updateRomFilesCmd.Parameters.Add(param);

                    updateRomFilesCmd.ExecuteNonQuery();
                }

                using (var updateHistoryCmd = conn.CreateCommand())
                {
                    updateHistoryCmd.Transaction = tran;
                    updateHistoryCmd.CommandText = $"UPDATE \"History\" SET \"Language\" = ? WHERE \"SeriesId\" IN ({gameIds})";
                    var param = updateHistoryCmd.CreateParameter();
                    languageConverter.SetValue(param, language);
                    updateHistoryCmd.Parameters.Add(param);

                    updateHistoryCmd.ExecuteNonQuery();
                }

                using (var updateBlacklistCmd = conn.CreateCommand())
                {
                    updateBlacklistCmd.Transaction = tran;
                    updateBlacklistCmd.CommandText = $"UPDATE \"Blacklist\" SET \"Language\" = ? WHERE \"SeriesId\" IN ({gameIds})";
                    var param = updateBlacklistCmd.CreateParameter();
                    languageConverter.SetValue(param, language);
                    updateBlacklistCmd.Parameters.Add(param);

                    updateBlacklistCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
