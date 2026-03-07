using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Playarr.Core.Datastore.Converters;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(161)]
    public class remove_plex_hometheatre : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexHomeTheater" });
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexClient" });

            // Switch Quality and Language to int in pending releases
            Execute.WithConnection(FixPendingReleases);
        }

        private void FixPendingReleases(IDbConnection conn, IDbTransaction tran)
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedRomInfo161>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedRomInfo162>());
            var rows = conn.Query<ParsedRomInfoData161>("SELECT \"Id\", \"ParsedRomInfo\" from \"PendingReleases\"");

            var newRows = new List<ParsedRomInfoData162>();

            foreach (var row in rows)
            {
                var old = row.ParsedRomInfo;

                var newQuality = new QualityModel162
                {
                    Quality = old.Quality.Quality.Id,
                    Revision = old.Quality.Revision
                };

                var correct = new ParsedRomInfo162
                {
                    GameTitle = old.GameTitle,
                    GameTitleInfo = old.GameTitleInfo,
                    Quality = newQuality,
                    SeasonNumber = old.SeasonNumber,
                    SeasonPart = old.SeasonPart,
                    RomNumbers = old.RomNumbers,
                    AbsoluteRomNumbers = old.AbsoluteRomNumbers,
                    SpecialAbsoluteRomNumbers = old.SpecialAbsoluteRomNumbers,
                    Language = old.Language?.Id ?? 0,
                    FullSeason = old.FullSeason,
                    IsPartialSeason = old.IsPartialSeason,
                    IsMultiSeason = old.IsMultiSeason,
                    IsSeasonExtra = old.IsSeasonExtra,
                    Speacial = old.Speacial,
                    ReleaseGroup = old.ReleaseGroup,
                    ReleaseHash = old.ReleaseHash,
                    ReleaseTokens = old.ReleaseTokens,
                    IsDaily = old.IsDaily,
                    IsAbsoluteNumbering = old.IsAbsoluteNumbering,
                    IsPossibleSpecialEpisode = old.IsPossibleSpecialEpisode,
                    IsPossibleSceneSeasonSpecial = old.IsPossibleSceneSeasonSpecial
                };

                newRows.Add(new ParsedRomInfoData162
                {
                    Id = row.Id,
                    ParsedRomInfo = correct
                });
            }

            var sql = $"UPDATE \"PendingReleases\" SET \"ParsedRomInfo\" = @ParsedRomInfo WHERE \"Id\" = @Id";

            conn.Execute(sql, newRows, transaction: tran);
        }

        private class ParsedRomInfoData161 : ModelBase
        {
            public ParsedRomInfo161 ParsedRomInfo { get; set; }
        }

        private class ParsedRomInfo161
        {
            public string GameTitle { get; set; }
            public GameTitleInfo161 GameTitleInfo { get; set; }
            public QualityModel161 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> RomNumbers { get; set; }
            public List<int> AbsoluteRomNumbers { get; set; }
            public List<int> SpecialAbsoluteRomNumbers { get; set; }
            public Language161 Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class GameTitleInfo161
        {
            public string Title { get; set; }
            public string TitleWithoutYear { get; set; }
            public int Year { get; set; }
        }

        private class QualityModel161
        {
            public Quality161 Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Language161
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Quality161
        {
            public int Id { get; set; }
        }

        private class ParsedRomInfoData162 : ModelBase
        {
            public ParsedRomInfo162 ParsedRomInfo { get; set; }
        }

        private class ParsedRomInfo162
        {
            public string GameTitle { get; set; }
            public GameTitleInfo161 GameTitleInfo { get; set; }
            public QualityModel162 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> RomNumbers { get; set; }
            public List<int> AbsoluteRomNumbers { get; set; }
            public List<int> SpecialAbsoluteRomNumbers { get; set; }
            public int Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class QualityModel162
        {
            public int Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Revision162
        {
            public int Version { get; set; }
            public int Real { get; set; }
            public bool IsRepack { get; set; }
        }
    }
}
