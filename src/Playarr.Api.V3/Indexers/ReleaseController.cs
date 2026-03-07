using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Playarr.Common.Cache;
using Playarr.Common.EnsureThat;
using Playarr.Common.Extensions;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.Exceptions;
using Playarr.Core.Indexers;
using Playarr.Core.IndexerSearch;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Games;
using Playarr.Core.Validation;
using Playarr.Http;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Playarr.Api.V3.Indexers
{
    [V3ApiController]
    public class ReleaseController : ReleaseControllerBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IGameService _seriesService;
        private readonly IRomService _episodeService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        private readonly ICached<RemoteEpisode> _remoteRomCache;

        public ReleaseController(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForReleases releaseSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IGameService seriesService,
                             IRomService episodeService,
                             IParsingService parsingService,
                             ICacheManager cacheManager,
                             IQualityProfileService qualityProfileService,
                             Logger logger)
            : base(qualityProfileService)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _releaseSearchService = releaseSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _parsingService = parsingService;
            _logger = logger;

            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteRomCache = cacheManager.GetCache<RemoteEpisode>(GetType(), "remoteRoms");
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<object> DownloadRelease([FromBody] ReleaseResource release)
        {
            var remoteRom = _remoteRomCache.Find(GetCacheKey(release));

            if (remoteRom == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new PlayarrClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (release.ShouldOverride == true)
                {
                    Ensure.That(release.SeriesId, () => release.SeriesId).IsNotNull();
                    Ensure.That(release.RomIds, () => release.RomIds).IsNotNull();
                    Ensure.That(release.RomIds, () => release.RomIds).HasItems();
                    Ensure.That(release.Quality, () => release.Quality).IsNotNull();
                    Ensure.That(release.Languages, () => release.Languages).IsNotNull();

                    // Clone the remote rom so we don't overwrite anything on the original
                    remoteRom = new RemoteEpisode
                    {
                        Release = remoteRom.Release,
                        ParsedRomInfo = remoteRom.ParsedRomInfo.JsonClone(),
                        SceneMapping = remoteRom.SceneMapping,
                        MappedPlatformNumber = remoteRom.MappedPlatformNumber,
                        EpisodeRequested = remoteRom.EpisodeRequested,
                        DownloadAllowed = remoteRom.DownloadAllowed,
                        SeedConfiguration = remoteRom.SeedConfiguration,
                        CustomFormats = remoteRom.CustomFormats,
                        CustomFormatScore = remoteRom.CustomFormatScore,
                        SeriesMatchType = remoteRom.SeriesMatchType,
                        ReleaseSource = remoteRom.ReleaseSource
                    };

                    remoteRom.Game = _seriesService.GetSeries(release.SeriesId!.Value);
                    remoteRom.Roms = _episodeService.GetEpisodes(release.RomIds);
                    remoteRom.ParsedRomInfo.Quality = release.Quality;
                    remoteRom.Languages = release.Languages;
                }

                if (remoteRom.Game == null)
                {
                    if (release.EpisodeId.HasValue)
                    {
                        var rom = _episodeService.GetEpisode(release.EpisodeId.Value);

                        remoteRom.Game = _seriesService.GetSeries(rom.SeriesId);
                        remoteRom.Roms = new List<Rom> { rom };
                    }
                    else if (release.SeriesId.HasValue)
                    {
                        var game = _seriesService.GetSeries(release.SeriesId.Value);
                        var roms = _parsingService.GetEpisodes(remoteRom.ParsedRomInfo, game, true);

                        if (roms.Empty())
                        {
                            throw new PlayarrClientException(HttpStatusCode.NotFound, "Unable to parse roms in the release, will need to be manually provided");
                        }

                        remoteRom.Game = game;
                        remoteRom.Roms = roms;
                    }
                    else
                    {
                        throw new PlayarrClientException(HttpStatusCode.NotFound, "Unable to find matching game and roms, will need to be manually provided");
                    }
                }
                else if (remoteRom.Roms.Empty())
                {
                    var roms = _parsingService.GetEpisodes(remoteRom.ParsedRomInfo, remoteRom.Game, true);

                    if (roms.Empty() && release.EpisodeId.HasValue)
                    {
                        var rom = _episodeService.GetEpisode(release.EpisodeId.Value);

                        roms = new List<Rom> { rom };
                    }

                    remoteRom.Roms = roms;
                }

                if (remoteRom.Roms.Empty())
                {
                    throw new PlayarrClientException(HttpStatusCode.NotFound, "Unable to parse roms in the release, will need to be manually provided");
                }

                await _downloadService.DownloadReport(remoteRom, release.DownloadClientId);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, ex.Message);
                throw new PlayarrClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<List<ReleaseResource>> GetReleases(int? gameId, int? romId, int? platformNumber)
        {
            if (romId.HasValue)
            {
                return await GetEpisodeReleases(romId.Value);
            }

            if (gameId.HasValue && platformNumber.HasValue)
            {
                return await GetSeasonReleases(gameId.Value, platformNumber.Value);
            }

            return await GetRss();
        }

        private async Task<List<ReleaseResource>> GetEpisodeReleases(int romId)
        {
            try
            {
                var decisions = await _releaseSearchService.EpisodeSearch(romId, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (SearchFailedException ex)
            {
                throw new PlayarrClientException(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Rom search failed: " + ex.Message);
                throw new PlayarrClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task<List<ReleaseResource>> GetSeasonReleases(int gameId, int platformNumber)
        {
            try
            {
                var decisions = await _releaseSearchService.SeasonSearch(gameId, platformNumber, false, false, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (SearchFailedException ex)
            {
                throw new PlayarrClientException(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Platform search failed: " + ex.Message);
                throw new PlayarrClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task<List<ReleaseResource>> GetRss()
        {
            var reports = await _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteRomCache.Set(GetCacheKey(resource), decision.RemoteEpisode, TimeSpan.FromMinutes(30));

            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
