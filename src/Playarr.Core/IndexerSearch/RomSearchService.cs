using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Queue;
using Playarr.Core.Games;

namespace Playarr.Core.IndexerSearch
{
    public class RomSearchService : IExecute<RomSearchCommand>,
                                        IExecute<MissingRomSearchCommand>,
                                        IExecute<CutoffUnmetRomSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IRomService _romService;
        private readonly IEpisodeCutoffService _romCutoffService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public RomSearchService(ISearchForReleases releaseSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    IRomService episodeService,
                                    IEpisodeCutoffService episodeCutoffService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _romService = episodeService;
            _romCutoffService = episodeCutoffService;
            _queueService = queueService;
            _logger = logger;
        }

        private async Task SearchForBulkEpisodes(List<Rom> roms, bool monitoredOnly, bool userInvokedSearch)
        {
            _logger.ProgressInfo("Performing search for {0} roms", roms.Count);
            var downloadedCount = 0;
            var groups = new List<RomSearchGroup>();

            foreach (var game in roms.GroupBy(e => e.GameId))
            {
                foreach (var platform in game.Select(e => e).GroupBy(e => e.PlatformNumber))
                {
                    groups.Add(new RomSearchGroup
                    {
                        GameId = game.Key,
                        PlatformNumber = platform.Key,
                        Roms = platform.ToList()
                    });
                }
            }

            foreach (var group in groups.OrderBy(g => g.Roms.Min(e => e.LastSearchTime ?? DateTime.MinValue)))
            {
                List<DownloadDecision> decisions;

                var gameId = group.GameId;
                var platformNumber = group.PlatformNumber;
                var groupRoms = group.Roms;

                if (groupRoms.Count > 1)
                {
                    try
                    {
                        decisions = await _releaseSearchService.PlatformSearch(gameId, platformNumber, groupRoms, monitoredOnly, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for roms in platform {0} of [{1}]", platformNumber, gameId);
                        continue;
                    }
                }
                else
                {
                    var rom = groupRoms.First();

                    try
                    {
                        decisions = await _releaseSearchService.RomSearch(rom, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for rom: [{0}]", rom);
                        continue;
                    }
                }

                var processed = await _processDownloadDecisions.ProcessDecisions(decisions);

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed search for {0} roms. {1} reports downloaded.", roms.Count, downloadedCount);
        }

        private bool IsMonitored(bool episodeMonitored, bool seriesMonitored)
        {
            return episodeMonitored && seriesMonitored;
        }

        public void Execute(RomSearchCommand message)
        {
            foreach (var romId in message.RomIds)
            {
                var decisions = _releaseSearchService.RomSearch(romId, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
                var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

                _logger.ProgressInfo("Rom search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }

        public void Execute(MissingRomSearchCommand message)
        {
            var monitored = message.Monitored;
            List<Rom> roms;

            if (message.GameId.HasValue)
            {
                roms = _romService.GetEpisodeBySeries(message.GameId.Value)
                                          .Where(e => e.Monitored == monitored &&
                                                 !e.HasFile &&
                                                 e.AirDateUtc.HasValue &&
                                                 e.AirDateUtc.Value.Before(DateTime.UtcNow))
                                          .ToList();
            }
            else
            {
                var pagingSpec = new PagingSpec<Rom>
                                 {
                                     Page = 1,
                                     PageSize = 1000000,
                                     SortDirection = SortDirection.Ascending,
                                     SortKey = "Id"
                                 };

                if (monitored)
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
                }
                else
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
                }

                roms = _romService.EpisodesWithoutFiles(pagingSpec).Records.ToList();
            }

            var queue = GetQueuedRomIds();
            var missing = roms.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(missing, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }

        public void Execute(CutoffUnmetRomSearchCommand message)
        {
            var monitored = message.Monitored;

            var pagingSpec = new PagingSpec<Rom>
                             {
                                 Page = 1,
                                 PageSize = 100000,
                                 SortDirection = SortDirection.Ascending,
                                 SortKey = "Id"
                             };

            if (message.GameId.HasValue)
            {
                var gameId = message.GameId.Value;
                pagingSpec.FilterExpressions.Add(v => v.GameId == gameId);
            }

            if (monitored)
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Game.Monitored == true);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Game.Monitored == false);
            }

            var roms = _romCutoffService.EpisodesWhereCutoffUnmet(pagingSpec).Records.ToList();
            var queue = GetQueuedRomIds();
            var cutoffUnmet = roms.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(cutoffUnmet, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }

        private List<int> GetQueuedRomIds()
        {
            return _queueService.GetQueue()
                .Where(q => q.Roms.Any())
                .SelectMany(q => q.Roms.Select(e => e.Id))
                .ToList();
        }
    }
}
