using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.GameStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeasonStatistics> SeriesStatistics();
        List<SeasonStatistics> SeriesStatistics(int gameId);
    }

    public class SeriesStatisticsRepository : ISeriesStatisticsRepository
    {
        private const string _selectEpisodesTemplate = "SELECT /**select**/ FROM \"Roms\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectRomFilesTemplate = "SELECT /**select**/ FROM \"RomFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public SeriesStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<SeasonStatistics> SeriesStatistics()
        {
            var time = DateTime.UtcNow;
            return MapResults(Query(EpisodesBuilder(time), _selectEpisodesTemplate),
                Query(RomFilesBuilder(), _selectRomFilesTemplate));
        }

        public List<SeasonStatistics> SeriesStatistics(int gameId)
        {
            var time = DateTime.UtcNow;

            return MapResults(Query(EpisodesBuilder(time).Where<Rom>(x => x.GameId == gameId), _selectEpisodesTemplate),
                Query(RomFilesBuilder().Where<RomFile>(x => x.GameId == gameId), _selectRomFilesTemplate));
        }

        private List<SeasonStatistics> MapResults(List<SeasonStatistics> episodesResult, List<SeasonStatistics> filesResult)
        {
            episodesResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.GameId == e.GameId & f.PlatformNumber == e.PlatformNumber);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return episodesResult;
        }

        private List<SeasonStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<SeasonStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder EpisodesBuilder(DateTime currentDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("currentDate", currentDate, null);

            var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";
            var falseIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "false" : "0";

            return new SqlBuilder(_database.DatabaseType)
            .Select($@"""Roms"".""GameId"" AS GameId,
                             ""Roms"".""PlatformNumber"",
                             COUNT(*) AS TotalEpisodeCount,
                             SUM(CASE WHEN ""AirDateUtc"" <= @currentDate OR ""EpisodeFileId"" > 0 THEN 1 ELSE 0 END) AS AvailableEpisodeCount,
                             SUM(CASE WHEN (""Monitored"" = {trueIndicator} AND ""AirDateUtc"" <= @currentDate) OR ""EpisodeFileId"" > 0 THEN 1 ELSE 0 END) AS EpisodeCount,
                             SUM(CASE WHEN ""EpisodeFileId"" > 0 THEN 1 ELSE 0 END) AS EpisodeFileCount,
                             SUM(CASE WHEN ""Monitored"" = {trueIndicator} THEN 1 ELSE 0 END) AS MonitoredEpisodeCount,
                             MIN(CASE WHEN ""AirDateUtc"" < @currentDate OR ""Monitored"" = {falseIndicator} THEN NULL ELSE ""AirDateUtc"" END) AS NextAiringString,
                             MAX(CASE WHEN ""AirDateUtc"" >= @currentDate OR ""Monitored"" = {falseIndicator} THEN NULL ELSE ""AirDateUtc"" END) AS PreviousAiringString,
                             MAX(""AirDate"") AS LastAiredString",
                parameters)
            .GroupBy<Rom>(x => x.GameId)
            .GroupBy<Rom>(x => x.PlatformNumber);
        }

        private SqlBuilder RomFilesBuilder()
        {
            if (_database.DatabaseType == DatabaseType.SQLite)
            {
                return new SqlBuilder(_database.DatabaseType)
                .Select(@"""GameId"",
                            ""PlatformNumber"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            GROUP_CONCAT(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<RomFile>(x => x.GameId)
                .GroupBy<RomFile>(x => x.PlatformNumber);
            }

            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""GameId"",
                            ""PlatformNumber"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            string_agg(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<RomFile>(x => x.GameId)
                .GroupBy<RomFile>(x => x.PlatformNumber);
        }
    }
}
