using System.Collections.Generic;
using System.Linq;
using Dapper;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Games
{
    public interface IGameRepository : IBasicRepository<Game>
    {
        bool SeriesPathExists(string path);
        Game FindByTitle(string cleanTitle);
        Game FindByTitle(string cleanTitle, int year);
        List<Game> FindByTitleInexact(string cleanTitle);
        Game FindByIgdbId(int igdbId);
        Game FindByMobyGamesId(int mobyGamesId);
        Game FindByImdbId(string imdbId);
        Game FindByPath(string path);
        List<int> AllSeriesIgdbIds();
        Dictionary<int, string> AllSeriesPaths();
        Dictionary<int, List<int>> AllGameTags();
    }

    public class GameRepository : BasicRepository<Game>, IGameRepository
    {
        public GameRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Query(c => c.Path == path).Any();
        }

        public Game FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var game = Query(s => s.CleanTitle == cleanTitle)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(game);
        }

        public Game FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var game = Query(s => s.CleanTitle == cleanTitle && s.Year == year).ToList();

            return ReturnSingleSeriesOrThrow(game);
        }

        public List<Game> FindByTitleInexact(string cleanTitle)
        {
            var builder = Builder().Where($"instr(@cleanTitle, \"Game\".\"CleanTitle\")", new { cleanTitle = cleanTitle });

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                builder = Builder().Where($"(strpos(@cleanTitle, \"Game\".\"CleanTitle\") > 0)", new { cleanTitle = cleanTitle });
            }

            return Query(builder).ToList();
        }

        public Game FindByIgdbId(int igdbId)
        {
            return Query(s => s.TvdbId == igdbId).SingleOrDefault();
        }

        public Game FindByMobyGamesId(int mobyGamesId)
        {
            return Query(s => s.MobyGamesId == mobyGamesId).SingleOrDefault();
        }

        public Game FindByImdbId(string imdbId)
        {
            return Query(s => s.ImdbId == imdbId).SingleOrDefault();
        }

        public Game FindByPath(string path)
        {
            return Query(s => s.Path == path)
                        .FirstOrDefault();
        }

        public List<int> AllSeriesIgdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TvdbId\" FROM \"Game\"").ToList();
            }
        }

        public Dictionary<int, string> AllSeriesPaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Game\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public Dictionary<int, List<int>> AllGameTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Tags\" AS Value FROM \"Game\" WHERE \"Tags\" IS NOT NULL";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        private Game ReturnSingleSeriesOrThrow(List<Game> game)
        {
            if (game.Count == 0)
            {
                return null;
            }

            if (game.Count == 1)
            {
                return game.First();
            }

            throw new MultipleSeriesFoundException(game, "Expected one game, but found {0}. Matching game: {1}", game.Count, string.Join(", ", game));
        }
    }
}
