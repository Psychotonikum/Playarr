using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Cache;
using Playarr.Common.Extensions;
using Playarr.Core.IndexerSearch;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public interface IEpisodeRefreshedService
    {
        void Search(int gameId);
    }

    public class EpisodeRefreshedService : IEpisodeRefreshedService, IHandle<RomInfoRefreshedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRomService _romService;
        private readonly Logger _logger;
        private readonly ICached<List<int>> _searchCache;

        public EpisodeRefreshedService(ICacheManager cacheManager,
                                   IManageCommandQueue commandQueueManager,
                                   IRomService episodeService,
                                   Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _romService = episodeService;
            _logger = logger;
            _searchCache = cacheManager.GetCache<List<int>>(GetType());
        }

        public void Search(int gameId)
        {
            var previouslyAired = _searchCache.Find(gameId.ToString());

            if (previouslyAired != null && previouslyAired.Any())
            {
                var missing = previouslyAired.Select(e => _romService.GetEpisode(e)).Where(e => !e.HasFile).ToList();

                if (missing.Any())
                {
                    _commandQueueManager.Push(new RomSearchCommand(missing.Select(e => e.Id).ToList()));
                }
            }

            _searchCache.Remove(gameId.ToString());
        }

        public void Handle(RomInfoRefreshedEvent message)
        {
            if (message.Game.AddOptions == null)
            {
                var toSearch = new List<int>();

                if (!message.Game.Monitored)
                {
                    _logger.Debug("Game is not monitored");
                    return;
                }

                var previouslyAired = message.Added.Where(a =>
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .Select(e => e.Id)
                    .ToList();

                if (previouslyAired.Empty())
                {
                    _logger.Debug("Newly added roms all air in the future");
                }

                toSearch.AddRange(previouslyAired);

                var absoluteRomNumberAdded = message.Updated.Where(a =>
                        a.AbsoluteRomNumberAdded &&
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .Select(e => e.Id)
                    .ToList();

                if (absoluteRomNumberAdded.Empty())
                {
                    _logger.Debug("No updated roms recently aired and had absolute rom number added");
                }

                toSearch.AddRange(absoluteRomNumberAdded);

                if (toSearch.Any())
                {
                    _searchCache.Set(message.Game.Id.ToString(), toSearch.Distinct().ToList());
                }
            }
        }
    }
}
