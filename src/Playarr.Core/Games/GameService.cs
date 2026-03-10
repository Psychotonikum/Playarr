using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.AutoTagging;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public interface IGameService
    {
        Game GetGame(int gameId);
        List<Game> GetGame(IEnumerable<int> gameIds);
        Game AddGame(Game newGame);
        List<Game> AddGame(List<Game> newGame);
        Game FindByIgdbId(int igdbId);
        Game FindByMobyGamesId(int mobyGamesId);
        Game FindByImdbId(string imdbId);
        Game FindByTitle(string title);
        Game FindByTitle(string title, int year);
        Game FindByTitleInexact(string title);
        Game FindByPath(string path);
        void DeleteGame(List<int> gameIds, bool deleteFiles, bool addImportListExclusion);
        List<Game> GetAllGames();
        List<int> AllGameIgdbIds();
        Dictionary<int, string> GetAllGamePaths();
        Dictionary<int, List<int>> GetAllGameTags();
        List<Game> AllForTag(int tagId);
        Game UpdateSeries(Game game, bool updateEpisodesToMatchSeason = true, bool publishUpdatedEvent = true);
        List<Game> UpdateSeries(List<Game> game, bool useExistingRelativeFolder);
        bool SeriesPathExists(string folder);
        void RemoveAddOptions(Game game);
        bool UpdateTags(Game game);
    }

    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRomService _romService;
        private readonly IBuildSeriesPaths _seriesPathBuilder;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public GameService(IGameRepository seriesRepository,
                             IEventAggregator eventAggregator,
                             IRomService episodeService,
                             IBuildSeriesPaths seriesPathBuilder,
                             IAutoTaggingService autoTaggingService,
                             Logger logger)
        {
            _gameRepository = seriesRepository;
            _eventAggregator = eventAggregator;
            _romService = episodeService;
            _seriesPathBuilder = seriesPathBuilder;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        public Game GetGame(int gameId)
        {
            return _gameRepository.Get(gameId);
        }

        public List<Game> GetGame(IEnumerable<int> gameIds)
        {
            return _gameRepository.Get(gameIds).ToList();
        }

        public Game AddGame(Game newGame)
        {
            _gameRepository.Insert(newGame);
            _eventAggregator.PublishEvent(new SeriesAddedEvent(GetGame(newGame.Id)));

            return newGame;
        }

        public List<Game> AddGame(List<Game> newGame)
        {
            _gameRepository.InsertMany(newGame);
            _eventAggregator.PublishEvent(new GameImportedEvent(newGame.Select(s => s.Id).ToList()));

            return newGame;
        }

        public Game FindByIgdbId(int mobyGamesId)
        {
            return _gameRepository.FindByIgdbId(mobyGamesId);
        }

        public Game FindByMobyGamesId(int mobyGamesId)
        {
            return _gameRepository.FindByMobyGamesId(mobyGamesId);
        }

        public Game FindByImdbId(string imdbId)
        {
            return _gameRepository.FindByImdbId(imdbId);
        }

        public Game FindByTitle(string title)
        {
            return _gameRepository.FindByTitle(title.CleanGameTitle());
        }

        public Game FindByTitleInexact(string title)
        {
            // find any game clean title within the provided release title
            var cleanTitle = title.CleanGameTitle();
            var list = _gameRepository.FindByTitleInexact(cleanTitle);
            if (!list.Any())
            {
                // no game matched
                return null;
            }

            if (list.Count == 1)
            {
                // return the first game if there is only one
                return list.Single();
            }

            // build ordered list of game by position in the search string
            var query =
                list.Select(game => new
                {
                    position = cleanTitle.IndexOf(game.CleanTitle),
                    length = game.CleanTitle.Length,
                    game = game
                })
                    .Where(s => (s.position >= 0))
                    .ToList()
                    .OrderBy(s => s.position)
                    .ThenByDescending(s => s.length)
                    .ToList();

            // get the leftmost game that is the longest
            // game are usually the first thing in release title, so we select the leftmost and longest match
            var match = query.First().game;

            _logger.Debug("Multiple game matched {0} from title {1}", match.Title, title);
            foreach (var entry in list)
            {
                _logger.Debug("Multiple game match candidate: {0} cleantitle: {1}", entry.Title, entry.CleanTitle);
            }

            return match;
        }

        public Game FindByPath(string path)
        {
            return _gameRepository.FindByPath(path);
        }

        public Game FindByTitle(string title, int year)
        {
            return _gameRepository.FindByTitle(title.CleanGameTitle(), year);
        }

        public void DeleteGame(List<int> gameIds, bool deleteFiles, bool addImportListExclusion)
        {
            var game = _gameRepository.Get(gameIds).ToList();
            _gameRepository.DeleteMany(gameIds);
            _eventAggregator.PublishEvent(new SeriesDeletedEvent(game, deleteFiles, addImportListExclusion));
        }

        public List<Game> GetAllGames()
        {
            return _gameRepository.All().ToList();
        }

        public List<int> AllGameIgdbIds()
        {
            return _gameRepository.AllGameIgdbIds().ToList();
        }

        public Dictionary<int, string> GetAllGamePaths()
        {
            return _gameRepository.AllGamePaths();
        }

        public Dictionary<int, List<int>> GetAllGameTags()
        {
            return _gameRepository.AllGameTags();
        }

        public List<Game> AllForTag(int tagId)
        {
            return GetAllGames().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        // updateEpisodesToMatchSeason is an override for EpisodeMonitoredService to use so a change via Platform pass doesn't get nuked by the platforms loop.
        // TODO: Remove when platforms are split from game (or we come up with a better way to address this)
        public Game UpdateSeries(Game game, bool updateEpisodesToMatchSeason = true, bool publishUpdatedEvent = true)
        {
            var storedGame = GetGame(game.Id);

            var episodeMonitoredChanged = false;

            if (updateEpisodesToMatchSeason)
            {
                foreach (var platform in game.Platforms)
                {
                    var storedSeason = storedGame.Platforms.SingleOrDefault(s => s.PlatformNumber == platform.PlatformNumber);

                    if (storedSeason != null && platform.Monitored != storedSeason.Monitored)
                    {
                        _romService.SetEpisodeMonitoredBySeason(game.Id, platform.PlatformNumber, platform.Monitored);
                        episodeMonitoredChanged = true;
                    }
                }
            }

            // Never update AddOptions when updating a game, keep it the same as the existing stored game.
            game.AddOptions = storedGame.AddOptions;
            UpdateTags(game);

            var updatedSeries = _gameRepository.Update(game);
            if (publishUpdatedEvent)
            {
                _eventAggregator.PublishEvent(new SeriesEditedEvent(updatedSeries, storedGame, episodeMonitoredChanged));
            }

            return updatedSeries;
        }

        public List<Game> UpdateSeries(List<Game> game, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} game", game.Count);

            foreach (var s in game)
            {
                _logger.Trace("Updating: {0}", s.Title);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _seriesPathBuilder.BuildPath(s, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", s.Title, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Title);
                }

                UpdateTags(s);
            }

            _gameRepository.UpdateMany(game);
            _logger.Debug("{0} game updated", game.Count);
            _eventAggregator.PublishEvent(new SeriesBulkEditedEvent(game));

            return game;
        }

        public bool SeriesPathExists(string folder)
        {
            return _gameRepository.SeriesPathExists(folder);
        }

        public void RemoveAddOptions(Game game)
        {
            _gameRepository.SetFields(game, s => s.AddOptions);
        }

        public bool UpdateTags(Game game)
        {
            _logger.Trace("Updating tags for {0}", game);

            var tagsAdded = new HashSet<int>();
            var tagsRemoved = new HashSet<int>();
            var changes = _autoTaggingService.GetTagChanges(game);

            foreach (var tag in changes.TagsToRemove)
            {
                if (game.Tags.Contains(tag))
                {
                    game.Tags.Remove(tag);
                    tagsRemoved.Add(tag);
                }
            }

            foreach (var tag in changes.TagsToAdd)
            {
                if (!game.Tags.Contains(tag))
                {
                    game.Tags.Add(tag);
                    tagsAdded.Add(tag);
                }
            }

            if (tagsAdded.Any() || tagsRemoved.Any())
            {
                _logger.Debug("Updated tags for '{0}'. Added: {1}, Removed: {2}", game.Title, tagsAdded.Count, tagsRemoved.Count);

                return true;
            }

            _logger.Debug("Tags not updated for '{0}'", game.Title);

            return false;
        }
    }
}
