using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.CustomFormats;
using Playarr.Core.Datastore.Events;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Exceptions;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;
using BadRequestException = Playarr.Http.REST.BadRequestException;

namespace Playarr.Api.V3.RomFiles
{
    [V3ApiController]
    public class RomFileController : RestControllerWithSignalR<RomFileResource, RomFile>,
                                 IHandle<RomFileAddedEvent>,
                                 IHandle<RomFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IGameService _seriesService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public RomFileController(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDeleteMediaFiles mediaFileDeletionService,
                             IGameService seriesService,
                             ICustomFormatCalculationService formatCalculator,
                             IUpgradableSpecification upgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _seriesService = seriesService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
        }

        protected override RomFileResource GetResourceById(int id)
        {
            var romFile = _mediaFileService.Get(id);
            var game = _seriesService.GetSeries(romFile.GameId);

            var resource = romFile.ToResource(game, _upgradableSpecification, _formatCalculator);

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<RomFileResource> GetRomFiles(int? gameId, [FromQuery] List<int> romFileIds)
        {
            if (!gameId.HasValue && !romFileIds.Any())
            {
                throw new BadRequestException("gameId or romFileIds must be provided");
            }

            if (gameId.HasValue)
            {
                var game = _seriesService.GetSeries(gameId.Value);
                var files = _mediaFileService.GetFilesBySeries(gameId.Value);

                if (files == null)
                {
                    return new List<RomFileResource>();
                }

                return files.ConvertAll(e => e.ToResource(game, _upgradableSpecification, _formatCalculator))
                            .ToList();
            }
            else
            {
                var romFiles = _mediaFileService.Get(romFileIds);

                return romFiles.GroupBy(e => e.GameId)
                                   .SelectMany(f => f.ToList()
                                                     .ConvertAll(e => e.ToResource(_seriesService.GetSeries(f.Key), _upgradableSpecification, _formatCalculator)))
                                   .ToList();
            }
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<RomFileResource> SetQuality([FromBody] RomFileResource romFileResource)
        {
            var romFile = _mediaFileService.Get(romFileResource.Id);
            romFile.Quality = romFileResource.Quality;

            if (romFileResource.SceneName != null && SceneChecker.IsSceneTitle(romFileResource.SceneName))
            {
                romFile.SceneName = romFileResource.SceneName;
            }

            if (romFileResource.ReleaseGroup != null)
            {
                romFile.ReleaseGroup = romFileResource.ReleaseGroup;
            }

            _mediaFileService.Update(romFile);
            return Accepted(romFile.Id);
        }

        [Obsolete("Use bulk endpoint instead")]
        [HttpPut("editor")]
        [Consumes("application/json")]
        public object SetQuality([FromBody] RomFileListResource resource)
        {
            var romFiles = _mediaFileService.GetFiles(resource.RomFileIds);

            foreach (var romFile in romFiles)
            {
                if (resource.Languages != null)
                {
                    romFile.Languages = resource.Languages;
                }

                if (resource.Quality != null)
                {
                    romFile.Quality = resource.Quality;
                }

                if (resource.SceneName != null && SceneChecker.IsSceneTitle(resource.SceneName))
                {
                    romFile.SceneName = resource.SceneName;
                }

                if (resource.ReleaseGroup != null)
                {
                    romFile.ReleaseGroup = resource.ReleaseGroup;
                }
            }

            _mediaFileService.Update(romFiles);

            var game = _seriesService.GetSeries(romFiles.First().GameId);

            return Accepted(romFiles.ConvertAll(f => f.ToResource(game, _upgradableSpecification, _formatCalculator)));
        }

        [RestDeleteById]
        public void DeleteRomFile(int id)
        {
            var romFile = _mediaFileService.Get(id);

            if (romFile == null)
            {
                throw new PlayarrClientException(HttpStatusCode.NotFound, "Rom file not found");
            }

            var game = _seriesService.GetSeries(romFile.GameId);

            _mediaFileDeletionService.DeleteRomFile(game, romFile);
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public object DeleteRomFiles([FromBody] RomFileListResource resource)
        {
            var romFiles = _mediaFileService.GetFiles(resource.RomFileIds);
            var game = _seriesService.GetSeries(romFiles.First().GameId);

            foreach (var romFile in romFiles)
            {
                _mediaFileDeletionService.DeleteRomFile(game, romFile);
            }

            return new { };
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        public object SetPropertiesBulk([FromBody] List<RomFileResource> resources)
        {
            var romFiles = _mediaFileService.GetFiles(resources.Select(r => r.Id));

            foreach (var romFile in romFiles)
            {
                var resourceRomFile = resources.Single(r => r.Id == romFile.Id);

                if (resourceRomFile.Languages != null)
                {
                    // Don't allow user to set files with 'Original' language
                    romFile.Languages = resourceRomFile.Languages.Where(l => l != null && l != Language.Original).ToList();
                }

                if (resourceRomFile.Quality != null)
                {
                    romFile.Quality = resourceRomFile.Quality;
                }

                if (resourceRomFile.SceneName != null && SceneChecker.IsSceneTitle(resourceRomFile.SceneName))
                {
                    romFile.SceneName = resourceRomFile.SceneName;
                }

                if (resourceRomFile.ReleaseGroup != null)
                {
                    romFile.ReleaseGroup = resourceRomFile.ReleaseGroup;
                }

                if (resourceRomFile.IndexerFlags.HasValue)
                {
                    romFile.IndexerFlags = (IndexerFlags)resourceRomFile.IndexerFlags;
                }

                if (resourceRomFile.ReleaseType != null)
                {
                    romFile.ReleaseType = (ReleaseType)resourceRomFile.ReleaseType;
                }
            }

            _mediaFileService.Update(romFiles);

            var game = _seriesService.GetSeries(romFiles.First().GameId);

            return Accepted(romFiles.ConvertAll(f => f.ToResource(game, _upgradableSpecification, _formatCalculator)));
        }

        [NonAction]
        public void Handle(RomFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.RomFile.Id);
        }

        [NonAction]
        public void Handle(RomFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.RomFile.Id);
        }
    }
}
