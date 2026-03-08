using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.CustomFormats;
using Playarr.Core.Datastore.Events;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;
using Playarr.SignalR;
using Playarr.Api.V3.RomFiles;
using Playarr.Api.V3.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Roms
{
    public abstract class RomControllerWithSignalR : RestControllerWithSignalR<RomResource, Rom>,
                                                         IHandle<EpisodeGrabbedEvent>,
                                                         IHandle<EpisodeImportedEvent>,
                                                         IHandle<RomFileDeletedEvent>
    {
        protected readonly IRomService _episodeService;
        protected readonly IGameService _seriesService;
        protected readonly IUpgradableSpecification _upgradableSpecification;
        protected readonly ICustomFormatCalculationService _formatCalculator;

        protected RomControllerWithSignalR(IRomService episodeService,
                                           IGameService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected RomControllerWithSignalR(IRomService episodeService,
                                           IGameService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected override RomResource GetResourceById(int id)
        {
            var rom = _episodeService.GetEpisode(id);
            var resource = MapToResource(rom, true, true, true);
            return resource;
        }

        protected RomResource MapToResource(Rom rom, bool includeSeries, bool includeRomFile, bool includeImages)
        {
            var resource = rom.ToResource();

            if (includeSeries || includeRomFile || includeImages)
            {
                var game = rom.Game ?? _seriesService.GetSeries(rom.GameId);

                if (includeSeries)
                {
                    resource.Game = game.ToResource();
                }

                if (includeRomFile && rom.EpisodeFileId != 0)
                {
                    resource.RomFile = rom.RomFile.Value.ToResource(game, _upgradableSpecification, _formatCalculator);
                }

                if (includeImages)
                {
                    resource.Images = rom.Images;
                }
            }

            return resource;
        }

        protected List<RomResource> MapToResource(List<Rom> roms, bool includeSeries, bool includeRomFile, bool includeImages)
        {
            var result = roms.ToResource();

            if (includeSeries || includeRomFile || includeImages)
            {
                var seriesDict = new Dictionary<int, Playarr.Core.Games.Game>();
                for (var i = 0; i < roms.Count; i++)
                {
                    var rom = roms[i];
                    var resource = result[i];

                    var game = rom.Game ?? seriesDict.GetValueOrDefault(roms[i].GameId) ?? _seriesService.GetSeries(roms[i].GameId);
                    seriesDict[game.Id] = game;

                    if (includeSeries)
                    {
                        resource.Game = game.ToResource();
                    }

                    if (includeRomFile && rom.EpisodeFileId != 0)
                    {
                        resource.RomFile = rom.RomFile.Value.ToResource(game, _upgradableSpecification, _formatCalculator);
                    }

                    if (includeImages)
                    {
                        resource.Images = rom.Images;
                    }
                }
            }

            return result;
        }

        [NonAction]
        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var rom in message.Rom.Roms)
            {
                var resource = rom.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        [NonAction]
        public void Handle(EpisodeImportedEvent message)
        {
            foreach (var rom in message.RomInfo.Roms)
            {
                BroadcastResourceChange(ModelAction.Updated, rom.Id);
            }
        }

        [NonAction]
        public void Handle(RomFileDeletedEvent message)
        {
            foreach (var rom in message.RomFile.Roms.Value)
            {
                BroadcastResourceChange(ModelAction.Updated, rom.Id);
            }
        }
    }
}
