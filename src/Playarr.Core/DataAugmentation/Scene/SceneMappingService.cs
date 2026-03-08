using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Playarr.Common.Cache;
using Playarr.Common.Extensions;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Games.Events;

namespace Playarr.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        List<string> GetSceneNames(int igdbId, List<int> platformNumbers, List<int> scenePlatformNumbers);
        int? FindIgdbId(string sceneTitle, string releaseTitle, int scenePlatformNumber);
        List<SceneMapping> FindByIgdbId(int igdbId);
        SceneMapping FindSceneMapping(string sceneTitle, string releaseTitle, int scenePlatformNumber);
        int? GetScenePlatformNumber(string gameTitle, string releaseTitle);
    }

    public class SceneMappingService : ISceneMappingService,
                                       IHandle<SeriesRefreshStartingEvent>,
                                       IHandle<SeriesAddedEvent>,
                                       IHandle<GameImportedEvent>,
                                       IExecute<UpdateSceneMappingCommand>
    {
        private readonly ISceneMappingRepository _repository;
        private readonly IEnumerable<ISceneMappingProvider> _sceneMappingProviders;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private readonly ICachedDictionary<List<SceneMapping>> _getIgdbIdCache;
        private readonly ICachedDictionary<List<SceneMapping>> _findByIgdbIdCache;
        private bool _updatedAfterStartup;

        public SceneMappingService(ISceneMappingRepository repository,
                                   ICacheManager cacheManager,
                                   IEnumerable<ISceneMappingProvider> sceneMappingProviders,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _repository = repository;
            _sceneMappingProviders = sceneMappingProviders;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _getIgdbIdCache = cacheManager.GetCacheDictionary<List<SceneMapping>>(GetType(), "igdb_id");
            _findByIgdbIdCache = cacheManager.GetCacheDictionary<List<SceneMapping>>(GetType(), "find_igdb_id");
        }

        public List<string> GetSceneNames(int igdbId, List<int> platformNumbers, List<int> scenePlatformNumbers)
        {
            var mappings = FindByIgdbId(igdbId);

            if (mappings == null)
            {
                return new List<string>();
            }

            var names = mappings.Where(n => platformNumbers.Contains(n.PlatformNumber ?? -1) ||
                                            scenePlatformNumbers.Contains(n.ScenePlatformNumber ?? -1) ||
                                            ((n.PlatformNumber ?? -1) == -1 && (n.ScenePlatformNumber ?? -1) == -1 && n.SceneOrigin != "igdb"))
                                .Select(n => n.SearchTerm)
                                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                .ToList();

            return names;
        }

        public int? FindIgdbId(string gameTitle, string releaseTitle, int scenePlatformNumber)
        {
            return FindSceneMapping(gameTitle, releaseTitle, scenePlatformNumber)?.IgdbId;
        }

        public List<SceneMapping> FindByIgdbId(int igdbId)
        {
            if (_findByIgdbIdCache.Count == 0)
            {
                RefreshCache();
            }

            var mappings = _findByIgdbIdCache.Find(igdbId.ToString());

            if (mappings == null)
            {
                return new List<SceneMapping>();
            }

            return mappings;
        }

        public SceneMapping FindSceneMapping(string gameTitle, string releaseTitle, int scenePlatformNumber)
        {
            if (gameTitle.IsNullOrWhiteSpace())
            {
                return null;
            }

            var mappings = FindMappings(gameTitle, releaseTitle);

            if (mappings == null)
            {
                return null;
            }

            mappings = FilterSceneMappings(mappings, scenePlatformNumber);

            var distinctMappings = mappings.DistinctBy(v => v.IgdbId).ToList();

            if (distinctMappings.Count == 0)
            {
                return null;
            }

            if (distinctMappings.Count == 1)
            {
                var mapping = distinctMappings.First();
                _logger.Debug("Found scene mapping for: {0}. IGDB ID for mapping: {1}", gameTitle, mapping.IgdbId);
                return distinctMappings.First();
            }

            throw new InvalidSceneMappingException(mappings, releaseTitle);
        }

        public int? GetScenePlatformNumber(string gameTitle, string releaseTitle)
        {
            return FindSceneMapping(gameTitle, releaseTitle, -1)?.ScenePlatformNumber;
        }

        private void UpdateMappings()
        {
            _logger.Info("Updating Scene mappings");

            _updatedAfterStartup = true;

            foreach (var sceneMappingProvider in _sceneMappingProviders)
            {
                try
                {
                    var mappings = sceneMappingProvider.GetSceneMappings();

                    if (mappings.Any())
                    {
                        _repository.Clear(sceneMappingProvider.GetType().Name);

                        mappings.RemoveAll(sceneMapping =>
                        {
                            if (sceneMapping.Title.IsNullOrWhiteSpace() ||
                                sceneMapping.SearchTerm.IsNullOrWhiteSpace())
                            {
                                _logger.Warn("Invalid scene mapping found for: {0}, skipping", sceneMapping.IgdbId);
                                return true;
                            }

                            return false;
                        });

                        foreach (var sceneMapping in mappings)
                        {
                            sceneMapping.ParseTerm = sceneMapping.Title.CleanGameTitle();
                            sceneMapping.Type = sceneMappingProvider.GetType().Name;
                        }

                        _repository.InsertMany(mappings.ToList());
                    }
                    else
                    {
                        _logger.Warn("Received empty list of mapping. will not update");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to Update Scene Mappings");
                }
            }

            RefreshCache();

            _eventAggregator.PublishEvent(new SceneMappingsUpdatedEvent());
        }

        private List<SceneMapping> FindMappings(string gameTitle, string releaseTitle)
        {
            if (_getIgdbIdCache.Count == 0)
            {
                RefreshCache();
            }

            var candidates = _getIgdbIdCache.Find(gameTitle.CleanGameTitle());

            if (candidates == null)
            {
                return null;
            }

            candidates = FilterSceneMappings(candidates, releaseTitle);

            if (candidates.Count <= 1)
            {
                return candidates;
            }

            var exactMatch = candidates.OrderByDescending(v => v.PlatformNumber)
                                       .Where(v => v.Title == gameTitle)
                                       .ToList();

            if (exactMatch.Any())
            {
                return exactMatch;
            }

            var closestMatch = candidates.OrderBy(v => gameTitle.LevenshteinDistance(v.Title, 10, 1, 10))
                                         .ThenByDescending(v => v.PlatformNumber)
                                         .First();

            return candidates.Where(v => v.Title == closestMatch.Title).ToList();
        }

        private void RefreshCache()
        {
            var mappings = _repository.All().ToList();

            _getIgdbIdCache.Update(mappings.GroupBy(v => v.ParseTerm).ToDictionary(v => v.Key, v => v.ToList()));
            _findByIgdbIdCache.Update(mappings.GroupBy(v => v.IgdbId).ToDictionary(v => v.Key.ToString(), v => v.ToList()));
        }

        private List<SceneMapping> FilterSceneMappings(List<SceneMapping> candidates, string releaseTitle)
        {
            var filteredCandidates = candidates.Where(v => v.FilterRegex.IsNotNullOrWhiteSpace()).ToList();
            var normalCandidates = candidates.Except(filteredCandidates).ToList();

            if (releaseTitle.IsNullOrWhiteSpace())
            {
                return normalCandidates;
            }

            var simpleTitle = Parser.Parser.SimplifyTitle(releaseTitle);

            filteredCandidates = filteredCandidates.Where(v => Regex.IsMatch(simpleTitle, v.FilterRegex)).ToList();

            if (filteredCandidates.Any())
            {
                return filteredCandidates;
            }

            return normalCandidates;
        }

        private List<SceneMapping> FilterSceneMappings(List<SceneMapping> candidates, int scenePlatformNumber)
        {
            var filteredCandidates = candidates.Where(v => (v.ScenePlatformNumber ?? -1) != -1 && (v.PlatformNumber ?? -1) != -1).ToList();
            var normalCandidates = candidates.Except(filteredCandidates).ToList();

            if (scenePlatformNumber == -1)
            {
                return normalCandidates;
            }

            if (filteredCandidates.Any())
            {
                filteredCandidates = filteredCandidates.Where(v => v.ScenePlatformNumber <= scenePlatformNumber)
                                                       .GroupBy(v => v.Title)
                                                       .Select(d => d.OrderByDescending(v => v.ScenePlatformNumber)
                                                                     .ThenByDescending(v => v.PlatformNumber)
                                                                     .First())
                                                       .ToList();

                return filteredCandidates;
            }

            return normalCandidates;
        }

        public void Handle(SeriesRefreshStartingEvent message)
        {
            if (message.ManualTrigger && (_findByIgdbIdCache.IsExpired(TimeSpan.FromMinutes(1)) || !_updatedAfterStartup))
            {
                UpdateMappings();
            }
        }

        public void Handle(SeriesAddedEvent message)
        {
            if (!_updatedAfterStartup)
            {
                UpdateMappings();
            }
        }

        public void Handle(GameImportedEvent message)
        {
            if (!_updatedAfterStartup)
            {
                UpdateMappings();
            }
        }

        public void Execute(UpdateSceneMappingCommand message)
        {
            UpdateMappings();
        }
    }
}
