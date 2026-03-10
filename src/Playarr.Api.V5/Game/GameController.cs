using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Common.TPL;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Datastore;
using Playarr.Core.Datastore.Events;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.RootFolders;
using Playarr.Core.GameStats;
using Playarr.Core.Games;
using Playarr.Core.Games.Commands;
using Playarr.Core.Games.Events;
using Playarr.Core.Validation;
using Playarr.Core.Validation.Paths;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;

namespace Playarr.Api.V5.Game;

[V5ApiController]
public class GameController : RestControllerWithSignalR<GameResource, Playarr.Core.Games.Game>,
                            IHandle<EpisodeImportedEvent>,
                            IHandle<RomFileDeletedEvent>,
                            IHandle<SeriesUpdatedEvent>,
                            IHandle<SeriesEditedEvent>,
                            IHandle<SeriesDeletedEvent>,
                            IHandle<SeriesRenamedEvent>,
                            IHandle<SeriesBulkEditedEvent>,
                            IHandle<MediaCoversUpdatedEvent>
{
    private readonly IGameService _gameService;
    private readonly IAddGameService _addGameService;
    private readonly ISeriesStatisticsService _seriesStatisticsService;
    private readonly ISceneMappingService _sceneMappingService;
    private readonly IMapCoversToLocal _coverMapper;
    private readonly IManageCommandQueue _commandQueueManager;
    private readonly IRootFolderService _rootFolderService;

    private readonly LockByIdPool _seriesLockPool = new();

    public GameController(IBroadcastSignalRMessage signalRBroadcaster,
                        IGameService seriesService,
                        IAddGameService addGameService,
                        ISeriesStatisticsService seriesStatisticsService,
                        ISceneMappingService sceneMappingService,
                        IMapCoversToLocal coverMapper,
                        IManageCommandQueue commandQueueManager,
                        IRootFolderService rootFolderService,
                        RootFolderValidator rootFolderValidator,
                        MappedNetworkDriveValidator mappedNetworkDriveValidator,
                        GamePathValidator seriesPathValidator,
                        GameExistsValidator seriesExistsValidator,
                        GameAncestorValidator seriesAncestorValidator,
                        SystemFolderValidator systemFolderValidator,
                        QualityProfileExistsValidator qualityProfileExistsValidator,
                        RootFolderExistsValidator rootFolderExistsValidator,
                        GameFolderAsRootFolderValidator seriesFolderAsRootFolderValidator)
        : base(signalRBroadcaster)
    {
        _gameService = seriesService;
        _addGameService = addGameService;
        _seriesStatisticsService = seriesStatisticsService;
        _sceneMappingService = sceneMappingService;

        _coverMapper = coverMapper;
        _commandQueueManager = commandQueueManager;
        _rootFolderService = rootFolderService;

        SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .IsValidPath()
            .SetValidator(rootFolderValidator)
            .SetValidator(mappedNetworkDriveValidator)
            .SetValidator(seriesPathValidator)
            .SetValidator(seriesAncestorValidator)
            .SetValidator(systemFolderValidator)
            .When(s => s.Path.IsNotNullOrWhiteSpace());

        PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath()
            .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
        PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath()
            .SetValidator(rootFolderExistsValidator)
            .SetValidator(seriesFolderAsRootFolderValidator)
            .When(s => s.Path.IsNullOrWhiteSpace());

        PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath();

        SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
            .ValidId()
            .SetValidator(qualityProfileExistsValidator);

        PostValidator.RuleFor(s => s.Title).NotEmpty();
        PostValidator.RuleFor(s => s.IgdbId).GreaterThan(0).SetValidator(seriesExistsValidator);
    }

    [HttpGet]
    [Produces("application/json")]
    public List<GameResource> AllSeries(int? igdbId, [FromQuery] SeriesSubresource[]? includeSubresources = null)
    {
        var seriesStats = _seriesStatisticsService.SeriesStatistics();
        var seriesResources = new List<GameResource>();
        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        if (igdbId.HasValue)
        {
            seriesResources.AddIfNotNull(_gameService.FindByIgdbId(igdbId.Value).ToResource(includeSeasonImages));
        }
        else
        {
            seriesResources.AddRange(_gameService.GetAllGames().Select(s => s.ToResource(includeSeasonImages)));
        }

        MapCoversToLocal(seriesResources.ToArray());
        LinkSeriesStatistics(seriesResources, seriesStats.ToDictionary(x => x.GameId));
        PopulateAlternateTitles(seriesResources);
        seriesResources.ForEach(LinkRootFolderPath);

        return seriesResources;
    }

    [NonAction]
    public override ActionResult<GameResource> GetResourceByIdWithErrorHandler(int id)
    {
        return base.GetResourceByIdWithErrorHandler(id);
    }

    [RestGetById]
    [Produces("application/json")]
    public ActionResult<GameResource> GetResourceByIdWithErrorHandler(int id, [FromQuery] SeriesSubresource[]? includeSubresources = null)
    {
        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        try
        {
            var game = GetGameResourceById(id, includeSeasonImages);

            return game == null ? NotFound() : game;
        }
        catch (ModelNotFoundException)
        {
            return NotFound();
        }
    }

    protected override GameResource? GetResourceById(int id)
    {
        var includeSubresources = Request?.Query["includeSubresources"].Select(v =>
        {
            if (Enum.TryParse<SeriesSubresource>(v, true, out var enumValue))
            {
                return enumValue;
            }

            throw new BadRequestException($"The value '{v}' is not valid.");
        }) ?? [];

        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        return GetGameResourceById(id, includeSeasonImages);
    }

    private GameResource? GetGameResourceById(int id, bool includeSeasonImages)
    {
        var game = _gameService.GetGame(id);

        return GetGameResource(game, includeSeasonImages);
    }

    [RestPostById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameResource> AddGame([FromBody] GameResource seriesResource)
    {
        var game = _addGameService.AddGame(seriesResource.ToModel());

        return Created(game.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameResource> UpdateSeries([FromBody] GameResource seriesResource, [FromQuery] bool moveFiles = false)
    {
        var game = _gameService.GetGame(seriesResource.Id);

        if (moveFiles)
        {
            var sourcePath = game.Path;
            var destinationPath = seriesResource.Path;

            _commandQueueManager.Push(new MoveGameCommand
            {
                GameId = game.Id,
                SourcePath = sourcePath,
                DestinationPath = destinationPath
            },
                trigger: CommandTrigger.Manual);
        }

        var model = seriesResource.ToModel(game);

        _gameService.UpdateSeries(model);

        BroadcastResourceChange(ModelAction.Updated, seriesResource);

        return Accepted(seriesResource.Id);
    }

    [HttpPut("{id}/platform")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<SeasonResource> UpdateSeasonMonitored([FromRoute] int id, [FromBody] SeasonResource seasonResource)
    {
        lock (_seriesLockPool.GetLock(id))
        {
            var game = _gameService.GetGame(id);
            var platform = game.Platforms.FirstOrDefault(s => s.PlatformNumber == seasonResource.PlatformNumber);

            if (platform == null)
            {
                return NotFound();
            }

            platform.Monitored = seasonResource.Monitored;

            _gameService.UpdateSeries(game);

            BroadcastResourceChange(ModelAction.Updated, game.ToResource());

            return platform.ToResource();
        }
    }

    [RestDeleteById]
    public ActionResult DeleteGame(int id, bool deleteFiles = false, bool addImportListExclusion = false)
    {
        _gameService.DeleteGame(new List<int> { id }, deleteFiles, addImportListExclusion);

        return NoContent();
    }

    private GameResource? GetGameResource(Playarr.Core.Games.Game? game, bool includeSeasonImages)
    {
        if (game == null)
        {
            return null;
        }

        var resource = game.ToResource(includeSeasonImages);
        MapCoversToLocal(resource);
        FetchAndLinkSeriesStatistics(resource);
        PopulateAlternateTitles(resource);
        LinkRootFolderPath(resource);

        return resource;
    }

    private void MapCoversToLocal(params GameResource[] game)
    {
        foreach (var seriesResource in game)
        {
            _coverMapper.ConvertToLocalUrls(seriesResource.Id, seriesResource.Images);
        }
    }

    private void FetchAndLinkSeriesStatistics(GameResource resource)
    {
        LinkSeriesStatistics(resource, _seriesStatisticsService.SeriesStatistics(resource.Id));
    }

    private void LinkSeriesStatistics(List<GameResource> resources, Dictionary<int, SeriesStatistics> seriesStatistics)
    {
        foreach (var game in resources)
        {
            if (seriesStatistics.TryGetValue(game.Id, out var stats))
            {
                LinkSeriesStatistics(game, stats);
            }
        }
    }

    private void LinkSeriesStatistics(GameResource resource, SeriesStatistics seriesStatistics)
    {
        // Only set last aired from statistics if it's missing from the game itself
        resource.LastAired ??= seriesStatistics.LastAired;

        resource.PreviousAiring = seriesStatistics.PreviousAiring;
        resource.NextAiring = seriesStatistics.NextAiring;
        resource.Statistics = seriesStatistics.ToResource(resource.Platforms);

        if (seriesStatistics.SeasonStatistics != null)
        {
            foreach (var platform in resource.Platforms)
            {
                platform.Statistics = seriesStatistics.SeasonStatistics?.SingleOrDefault(s => s.PlatformNumber == platform.PlatformNumber)?.ToResource();
            }
        }
    }

    private void PopulateAlternateTitles(List<GameResource> resources)
    {
        foreach (var resource in resources)
        {
            PopulateAlternateTitles(resource);
        }
    }

    private void PopulateAlternateTitles(GameResource resource)
    {
        var mappings = _sceneMappingService.FindByIgdbId(resource.IgdbId);

        if (mappings == null)
        {
            return;
        }

        resource.AlternateTitles = mappings.ConvertAll(AlternateTitleResourceMapper.ToResource);
    }

    private void LinkRootFolderPath(GameResource resource)
    {
        resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
    }

    [NonAction]
    public void Handle(EpisodeImportedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.GameId);
    }

    [NonAction]
    public void Handle(RomFileDeletedEvent message)
    {
        if (message.Reason == DeleteMediaFileReason.Upgrade)
        {
            return;
        }

        BroadcastResourceChange(ModelAction.Updated, message.RomFile.GameId);
    }

    [NonAction]
    public void Handle(SeriesUpdatedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
    }

    [NonAction]
    public void Handle(SeriesEditedEvent message)
    {
        var resource = GetGameResource(message.Game, false);

        if (resource == null)
        {
            return;
        }

        resource.EpisodesChanged = message.EpisodesChanged;
        BroadcastResourceChange(ModelAction.Updated, resource);
    }

    [NonAction]
    public void Handle(SeriesDeletedEvent message)
    {
        foreach (var game in message.Game)
        {
            var resource = GetGameResource(game, false);

            if (resource == null)
            {
                continue;
            }

            BroadcastResourceChange(ModelAction.Deleted, resource);
        }
    }

    [NonAction]
    public void Handle(SeriesRenamedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
    }

    [NonAction]
    public void Handle(SeriesBulkEditedEvent message)
    {
        foreach (var game in message.Game)
        {
            var resource = GetGameResource(game, false);

            if (resource == null)
            {
                continue;
            }

            BroadcastResourceChange(ModelAction.Updated, resource);
        }
    }

    [NonAction]
    public void Handle(MediaCoversUpdatedEvent message)
    {
        if (message.Updated)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Game.Id);
        }
    }
}
