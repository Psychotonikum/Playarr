using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NLog;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Qualities;

namespace Playarr.Core.Games
{
    public interface IRomRepository : IBasicRepository<Rom>
    {
        Rom Find(int gameId, int platform, int romNumber);
        Rom Find(int gameId, int absoluteRomNumber);
        List<Rom> Find(int gameId, string date);
        List<Rom> GetRoms(int gameId);
        List<Rom> GetRoms(int gameId, int platformNumber);
        List<Rom> GetRomsByGameIds(List<int> gameIds);
        List<Rom> GetRomsByScenePlatform(int gameId, int scenePlatformNumber);
        List<Rom> GetEpisodeByFileId(int fileId);
        List<Rom> EpisodesWithFiles(int gameId);
        PagingSpec<Rom> EpisodesWithoutFiles(PagingSpec<Rom> pagingSpec, bool includeSpecials);
        PagingSpec<Rom> EpisodesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials);
        List<Rom> FindEpisodesBySceneNumbering(int gameId, int platformNumber, int romNumber);
        List<Rom> FindEpisodesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber);
        List<Rom> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials);
        void SetMonitoredFlat(Rom rom, bool monitored);
        void SetMonitoredBySeason(int gameId, int platformNumber, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void SetFileId(Rom rom, int fileId);
        void ClearFileId(Rom rom, bool unmonitor);
    }

    public class RomRepository : BasicRepository<Rom>, IRomRepository
    {
        private readonly Logger _logger;

        public RomRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        protected override IEnumerable<Rom> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<Rom, Game>(builder, (rom, game) =>
            {
                rom.Game = game;
                return rom;
            });

        public Rom Find(int gameId, int platform, int romNumber)
        {
            return Query(s => s.GameId == gameId && s.PlatformNumber == platform && s.EpisodeNumber == romNumber)
                               .SingleOrDefault();
        }

        public Rom Find(int gameId, int absoluteRomNumber)
        {
            return Query(s => s.GameId == gameId && s.AbsoluteEpisodeNumber == absoluteRomNumber)
                        .SingleOrDefault();
        }

        public List<Rom> Find(int gameId, string date)
        {
            return Query(s => s.GameId == gameId && s.AirDate == date).ToList();
        }

        public List<Rom> GetRoms(int gameId)
        {
            return Query(s => s.GameId == gameId).ToList();
        }

        public List<Rom> GetRoms(int gameId, int platformNumber)
        {
            return Query(s => s.GameId == gameId && s.PlatformNumber == platformNumber).ToList();
        }

        public List<Rom> GetRomsByGameIds(List<int> gameIds)
        {
            return Query(s => gameIds.Contains(s.GameId)).ToList();
        }

        public List<Rom> GetRomsByScenePlatform(int gameId, int platformNumber)
        {
            return Query(s => s.GameId == gameId && s.ScenePlatformNumber == platformNumber).ToList();
        }

        public List<Rom> GetEpisodeByFileId(int fileId)
        {
            return Query(e => e.EpisodeFileId == fileId).ToList();
        }

        public List<Rom> EpisodesWithFiles(int gameId)
        {
            var builder = Builder()
                .Join<Rom, RomFile>((e, ef) => e.EpisodeFileId == ef.Id)
                .Where<Rom>(e => e.GameId == gameId);

            return _database.QueryJoined<Rom, RomFile>(
                builder,
                (rom, romFile) =>
                {
                    rom.RomFile = romFile;
                    return rom;
                }).ToList();
        }

        public PagingSpec<Rom> EpisodesWithoutFiles(PagingSpec<Rom> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingPlatformNumber = 1;

            if (includeSpecials)
            {
                startingPlatformNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWithoutFilesBuilder(currentTime, startingPlatformNumber), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWithoutFilesBuilder(currentTime, startingPlatformNumber).SelectCountDistinct<Rom>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        public PagingSpec<Rom> EpisodesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials)
        {
            var startingPlatformNumber = 1;

            if (includeSpecials)
            {
                startingPlatformNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingPlatformNumber), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Rom))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingPlatformNumber).Select(typeof(Rom)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Rom> FindEpisodesBySceneNumbering(int gameId, int platformNumber, int romNumber)
        {
            return Query(s => s.GameId == gameId && s.ScenePlatformNumber == platformNumber && s.SceneEpisodeNumber == romNumber).ToList();
        }

        public List<Rom> FindEpisodesBySceneNumbering(int gameId, int sceneAbsoluteRomNumber)
        {
            return Query(s => s.GameId == gameId && s.SceneAbsoluteEpisodeNumber == sceneAbsoluteRomNumber).ToList();
        }

        public List<Rom> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials)
        {
            var builder = Builder().Where<Rom>(rg => rg.AirDateUtc >= startDate && rg.AirDateUtc <= endDate);

            if (!includeSpecials)
            {
                builder = builder.Where<Rom>(e => e.PlatformNumber != 0);
            }

            if (!includeUnmonitored)
            {
                builder = builder.Where<Rom>(e => e.Monitored == true)
                    .Join<Rom, Game>((l, r) => l.GameId == r.Id)
                    .Where<Game>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Rom rom, bool monitored)
        {
            rom.Monitored = monitored;
            SetFields(rom, p => p.Monitored);

            ModelUpdated(rom, true);
        }

        public void SetMonitoredBySeason(int gameId, int platformNumber, bool monitored)
        {
            using (var conn = _database.OpenConnection())
            {
                conn.Execute("UPDATE \"Roms\" SET \"Monitored\" = @monitored WHERE \"GameId\" = @gameId AND \"PlatformNumber\" = @platformNumber AND \"Monitored\" != @monitored",
                    new { gameId = gameId, platformNumber = platformNumber, monitored = monitored });
            }
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var roms = ids.Select(x => new Rom { Id = x, Monitored = monitored }).ToList();
            SetFields(roms, p => p.Monitored);
        }

        public void SetFileId(Rom rom, int fileId)
        {
            rom.EpisodeFileId = fileId;

            SetFields(rom, ep => ep.EpisodeFileId);

            ModelUpdated(rom, true);
        }

        public void ClearFileId(Rom rom, bool unmonitor)
        {
            rom.EpisodeFileId = 0;
            rom.Monitored &= !unmonitor;

            SetFields(rom, ep => ep.EpisodeFileId, ep => ep.Monitored);

            ModelUpdated(rom, true);
        }

        private SqlBuilder EpisodesWithoutFilesBuilder(DateTime currentTime, int startingPlatformNumber) => Builder()
            .Join<Rom, Game>((l, r) => l.GameId == r.Id)
            .Where<Rom>(f => f.EpisodeFileId == 0)
            .Where<Rom>(f => f.PlatformNumber >= startingPlatformNumber)
            .Where(BuildAirDateUtcCutoffWhereClause(currentTime));

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                return string.Format("\"Roms\".\"AirDateUtc\" + make_interval(mins => \"Games\".\"Runtime\") <= '{0}'",
                                     currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return string.Format("datetime(strftime('%s', \"Roms\".\"AirDateUtc\") + \"Games\".\"Runtime\" * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private SqlBuilder EpisodesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff, int startingPlatformNumber) => Builder()
            .Join<Rom, Game>((e, s) => e.GameId == s.Id)
            .LeftJoin<Rom, RomFile>((e, ef) => e.EpisodeFileId == ef.Id)
            .Where<Rom>(e => e.EpisodeFileId != 0)
            .Where<Rom>(e => e.PlatformNumber >= startingPlatformNumber)
            .Where(
                string.Format("({0})",
                    BuildQualityCutoffWhereClause(qualitiesBelowCutoff)))
            .GroupBy<Rom>(e => e.Id)
            .GroupBy<Game>(s => s.Id);

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(\"Games\".\"QualityProfileId\" = {0} AND \"RomFiles\".\"Quality\" LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private Rom FindOneByAirDate(int gameId, string date)
        {
            var roms = Query(s => s.GameId == gameId && s.AirDate == date).ToList();

            if (!roms.Any())
            {
                return null;
            }

            if (roms.Count == 1)
            {
                return roms.First();
            }

            _logger.Debug("Multiple roms with the same air date were found, will exclude specials");

            var regularEpisodes = roms.Where(e => e.PlatformNumber > 0).ToList();

            if (regularEpisodes.Count == 1)
            {
                _logger.Debug("Left with one rom after excluding specials");
                return regularEpisodes.First();
            }

            throw new InvalidOperationException("Multiple roms with the same air date found");
        }
    }
}
