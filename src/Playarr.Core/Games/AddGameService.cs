using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using Playarr.Common.EnsureThat;
using Playarr.Common.Extensions;
using Playarr.Core.Exceptions;
using Playarr.Core.MetadataSource;
using Playarr.Core.Organizer;
using Playarr.Core.Parser;

namespace Playarr.Core.Games
{
    public interface IAddGameService
    {
        Game AddGame(Game newGame);
        List<Game> AddGame(List<Game> newGame, bool ignoreErrors = false);
    }

    public class AddGameService : IAddGameService
    {
        private readonly IGameService _seriesService;
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddGameValidator _addGameValidator;
        private readonly Logger _logger;

        public AddGameService(IGameService seriesService,
                                IProvideSeriesInfo seriesInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddGameValidator addGameValidator,
                                Logger logger)
        {
            _seriesService = seriesService;
            _seriesInfo = seriesInfo;
            _fileNameBuilder = fileNameBuilder;
            _addGameValidator = addGameValidator;
            _logger = logger;
        }

        public Game AddGame(Game newGame)
        {
            Ensure.That(newGame, () => newGame).IsNotNull();

            newGame = AddSkyhookData(newGame);
            newGame = SetPropertiesAndValidate(newGame);

            _logger.Info("Adding Game {0} Path: [{1}]", newGame, newGame.Path);
            _seriesService.AddGame(newGame);

            return newGame;
        }

        public List<Game> AddGame(List<Game> newGame, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var gamesToAdd = new List<Game>();
            var existingGameIgdbIds = _seriesService.AllSeriesIgdbIds();

            foreach (var s in newGame)
            {
                if (s.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Game {0} Root Folder Path: [{1}]", s, s.RootFolderPath);
                }
                else
                {
                    _logger.Info("Adding Game {0} Path: [{1}]", s, s.Path);
                }

                try
                {
                    var game = AddSkyhookData(s);
                    game = SetPropertiesAndValidate(game);
                    game.Added = added;
                    if (existingGameIgdbIds.Any(f => f == game.TvdbId))
                    {
                        _logger.Debug("IGDB ID {0} was not added due to validation failure: Game {1} already exists in database", s.TvdbId, s);
                        continue;
                    }

                    if (gamesToAdd.Any(f => f.TvdbId == game.TvdbId))
                    {
                        _logger.Trace("IGDB ID {0} was already added from another import list, not adding game {1} again", s.TvdbId, s);
                        continue;
                    }

                    var duplicateSlug = gamesToAdd.FirstOrDefault(f => f.TitleSlug == game.TitleSlug);
                    if (duplicateSlug != null)
                    {
                        _logger.Debug("IGDB ID {0} was not added due to validation failure: Duplicate Slug {1} used by game {2}", s.TvdbId, s.TitleSlug, duplicateSlug.TvdbId);
                        continue;
                    }

                    gamesToAdd.Add(game);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Game {0} with IGDB ID {1} was not added due to validation failures. {2}", s, s.TvdbId, ex.Message);
                }
            }

            return _seriesService.AddGame(gamesToAdd);
        }

        private Game AddSkyhookData(Game newGame)
        {
            Tuple<Game, List<Rom>> tuple;

            try
            {
                tuple = _seriesInfo.GetSeriesInfo(newGame.TvdbId);
            }
            catch (SeriesNotFoundException)
            {
                _logger.Error("Game {0} with IGDB ID {1} was not found, it may have been removed from TheIGDB. Path: {2}", newGame, newGame.TvdbId, newGame.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TvdbId", $"A game with this ID was not found. Path: {newGame.Path}", newGame.TvdbId)
                                              });
            }

            var game = tuple.Item1;

            // If platforms were passed in on the new game use them, otherwise use the platforms from Skyhook
            newGame.Platforms = newGame.Platforms != null && newGame.Platforms.Any() ? newGame.Platforms : game.Platforms;

            game.ApplyChanges(newGame);

            return game;
        }

        private Game SetPropertiesAndValidate(Game newGame)
        {
            if (string.IsNullOrWhiteSpace(newGame.Path))
            {
                var folderName = _fileNameBuilder.GetGameFolder(newGame);
                newGame.Path = Path.Combine(newGame.RootFolderPath, folderName);
            }

            newGame.CleanTitle = newGame.Title.CleanGameTitle();
            newGame.SortTitle = GameTitleNormalizer.Normalize(newGame.Title, newGame.TvdbId);
            newGame.Added = DateTime.UtcNow;

            if (newGame.AddOptions != null && newGame.AddOptions.Monitor == MonitorTypes.None)
            {
                newGame.Monitored = false;
            }

            var validationResult = _addGameValidator.Validate(newGame);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newGame;
        }
    }
}
