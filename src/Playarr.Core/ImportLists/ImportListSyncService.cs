using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.ImportLists.Exclusions;
using Playarr.Core.ImportLists.ImportListItems;
using Playarr.Core.Jobs;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.MetadataSource;
using Playarr.Core.Parser.Model;
using Playarr.Core.ThingiProvider.Events;
using Playarr.Core.Games;

namespace Playarr.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>, IHandleAsync<ProviderDeletedEvent<IImportList>>
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IImportListItemService _importListItemService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewSeries _seriesSearchService;
        private readonly IGameService _gameService;
        private readonly IAddGameService _addGameService;
        private readonly IConfigService _configService;
        private readonly ITaskManager _taskManager;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                              IImportListStatusService importListStatusService,
                              IImportListExclusionService importListExclusionService,
                              IImportListItemService importListItemService,
                              IFetchAndParseImportList listFetcherAndParser,
                              ISearchForNewSeries seriesSearchService,
                              IGameService seriesService,
                              IAddGameService addGameService,
                              IConfigService configService,
                              ITaskManager taskManager,
                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListStatusService = importListStatusService;
            _importListExclusionService = importListExclusionService;
            _importListItemService = importListItemService;
            _listFetcherAndParser = listFetcherAndParser;
            _seriesSearchService = seriesSearchService;
            _gameService = seriesService;
            _addGameService = addGameService;
            _configService = configService;
            _taskManager = taskManager;
            _logger = logger;
        }

        private bool AllListsSuccessfulWithAPendingClean()
        {
            var lists = _importListFactory.AutomaticAddEnabled(false);
            var anyRemoved = false;

            foreach (var list in lists)
            {
                var status = _importListStatusService.GetListStatus(list.Definition.Id);

                if (status.DisabledTill.HasValue)
                {
                    // list failed the last time it was synced.
                    return false;
                }

                if (!status.LastInfoSync.HasValue)
                {
                    // list has never been synced.
                    return false;
                }

                anyRemoved |= status.HasRemovedItemSinceLastClean;
            }

            return anyRemoved;
        }

        private void SyncAll()
        {
            if (_importListFactory.AutomaticAddEnabled().Empty())
            {
                _logger.Debug("No import lists with automatic add enabled");

                return;
            }

            _logger.ProgressInfo("Starting Import List Sync");

            var result = _listFetcherAndParser.Fetch();

            var listItems = result.Game.ToList();

            ProcessListItems(listItems);

            TryCleanLibrary();
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo("Starting Import List Refresh for List {0}", definition.Name);

            var result = _listFetcherAndParser.FetchSingleList(definition);

            var listItems = result.Game.ToList();

            ProcessListItems(listItems);

            TryCleanLibrary();
        }

        private void ProcessListItems(List<ImportListItemInfo> items)
        {
            var gamesToAdd = new List<Game>();

            if (items.Count == 0)
            {
                _logger.ProgressInfo("No list items to process");

                return;
            }

            _logger.ProgressInfo("Processing {0} list items", items.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();
            var importLists = _importListFactory.All();
            var existingIgdbIds = _gameService.AllGameIgdbIds();

            foreach (var item in items)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, items.Count);

                reportNumber++;

                var importList = importLists.Single(x => x.Id == item.ImportListId);

                if (!importList.EnableAutomaticAdd)
                {
                    continue;
                }

                // Map by IMDb ID if we have it
                if (item.IgdbId <= 0 && item.ImdbId.IsNotNullOrWhiteSpace())
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByImdbId(item.ImdbId)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.IgdbId = mappedSeries.IgdbId;
                        item.Title = mappedSeries?.Title;
                    }
                }

                // Map by TMDb ID if we have it
                if (item.IgdbId <= 0 && item.TmdbId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByTmdbId(item.TmdbId)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.IgdbId = mappedSeries.IgdbId;
                        item.Title = mappedSeries?.Title;
                    }
                }

                // Map by AniList ID if we have it
                if (item.IgdbId <= 0 && item.AniListId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByAniListId(item.AniListId)
                        .FirstOrDefault();

                    if (mappedSeries == null)
                    {
                        _logger.Debug("Rejected, unable to find matching IGDB ID for Anilist ID: {0} [{1}]", item.AniListId, item.Title);

                        continue;
                    }

                    item.IgdbId = mappedSeries.IgdbId;
                    item.Title = mappedSeries.Title;
                }

                // Map by MyAniList ID if we have it
                if (item.IgdbId <= 0 && item.MalId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByMyAnimeListId(item.MalId)
                        .FirstOrDefault();

                    if (mappedSeries == null)
                    {
                        _logger.Debug("Rejected, unable to find matching IGDB ID for MAL ID: {0} [{1}]", item.MalId, item.Title);

                        continue;
                    }

                    item.IgdbId = mappedSeries.IgdbId;
                    item.Title = mappedSeries.Title;
                }

                if (item.IgdbId == 0)
                {
                    _logger.Debug("[{0}] Rejected, unable to find IGDB ID", item.Title);
                    continue;
                }

                // Check to see if game excluded
                var excludedSeries = listExclusions.SingleOrDefault(s => s.IgdbId == item.IgdbId);

                if (excludedSeries != null)
                {
                    _logger.Debug("{0} [{1}] Rejected due to list exclusion", item.IgdbId, item.Title);
                    continue;
                }

                // Break if Game Exists in DB
                if (existingIgdbIds.Any(x => x == item.IgdbId))
                {
                    _logger.Debug("{0} [{1}] Rejected, game exists in database", item.IgdbId, item.Title);
                    continue;
                }

                // Append Game if not already in DB or already on add list
                if (gamesToAdd.All(s => s.IgdbId != item.IgdbId))
                {
                    var monitored = importList.ShouldMonitor != MonitorTypes.None;

                    gamesToAdd.Add(new Game
                    {
                        IgdbId = item.IgdbId,
                        Title = item.Title,
                        Year = item.Year,
                        Monitored = monitored,
                        MonitorNewItems = importList.MonitorNewItems,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.QualityProfileId,
                        SeriesType = importList.SeriesType,
                        PlatformFolder = importList.PlatformFolder,
                        Platforms = item.Platforms,
                        Tags = importList.Tags,
                        AddOptions = new AddGameOptions
                        {
                            SearchForMissingEpisodes = importList.SearchForMissingEpisodes,

                            // If platforms are provided use them for syncing monitored status, otherwise use the list setting.
                            Monitor = item.Platforms.Any() ? MonitorTypes.Skip : importList.ShouldMonitor
                        }
                    });
                }
            }

            _addGameService.AddGame(gamesToAdd, true);

            _logger.ProgressInfo("Import List Sync Completed. Items found: {0}, Game added: {1}", items.Count, gamesToAdd.Count);
        }

        public void Execute(ImportListSyncCommand message)
        {
            if (message.DefinitionId.HasValue)
            {
                SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                SyncAll();
            }
        }

        private void TryCleanLibrary()
        {
            if (_configService.ListSyncLevel == ListSyncLevelType.Disabled)
            {
                return;
            }

            if (AllListsSuccessfulWithAPendingClean())
            {
                CleanLibrary();
            }
        }

        private void CleanLibrary()
        {
            if (_configService.ListSyncLevel == ListSyncLevelType.Disabled)
            {
                return;
            }

            var gamesToUpdate = new List<Game>();
            var seriesInLibrary = _gameService.GetAllGames();
            var allListItems = _importListItemService.All();

            foreach (var game in seriesInLibrary)
            {
                var seriesExists = allListItems.Where(l =>
                    l.IgdbId == game.IgdbId ||
                    (l.ImdbId.IsNotNullOrWhiteSpace() && game.ImdbId.IsNotNullOrWhiteSpace() && l.ImdbId == game.ImdbId) ||
                    l.TmdbId == game.TmdbId ||
                    game.MalIds.Contains(l.MalId) ||
                    game.AniListIds.Contains(l.AniListId)).ToList();

                if (!seriesExists.Any())
                {
                    switch (_configService.ListSyncLevel)
                    {
                        case ListSyncLevelType.LogOnly:
                            _logger.Info("{0} was in your library, but not found in your lists --> You might want to unmonitor or remove it", game);
                            break;
                        case ListSyncLevelType.KeepAndUnmonitor when game.Monitored:
                            _logger.Info("{0} was in your library, but not found in your lists --> Keeping in library but unmonitoring it", game);
                            game.Monitored = false;
                            gamesToUpdate.Add(game);
                            break;
                        case ListSyncLevelType.KeepAndTag when !game.Tags.Contains(_configService.ListSyncTag):
                            _logger.Info("{0} was in your library, but not found in your lists --> Keeping in library but tagging it", game);
                            game.Tags.Add(_configService.ListSyncTag);
                            gamesToUpdate.Add(game);
                            break;
                        default:
                            break;
                    }
                }
            }

            _gameService.UpdateSeries(gamesToUpdate, true);
            _importListStatusService.MarkListsAsCleaned();
        }

        public void HandleAsync(ProviderDeletedEvent<IImportList> message)
        {
            TryCleanLibrary();
        }
    }
}
