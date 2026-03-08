using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Games;
using Playarr.Core.Games.Commands;
using Playarr.Http;

namespace Playarr.Api.V5.Game;

[V5ApiController("game/editor")]
public class GameEditorController : Controller
{
    private readonly IGameService _seriesService;
    private readonly IManageCommandQueue _commandQueueManager;
    private readonly GameEditorValidator _seriesEditorValidator;

    public GameEditorController(IGameService seriesService, IManageCommandQueue commandQueueManager, GameEditorValidator seriesEditorValidator)
    {
        _seriesService = seriesService;
        _commandQueueManager = commandQueueManager;
        _seriesEditorValidator = seriesEditorValidator;
    }

    [HttpPut]
    public object SaveAll([FromBody] GameEditorResource resource)
    {
        var gamesToUpdate = _seriesService.GetSeries(resource.GameIds);
        var seriesToMove = new List<BulkMoveGame>();

        foreach (var game in gamesToUpdate)
        {
            if (resource.Monitored.HasValue)
            {
                game.Monitored = resource.Monitored.Value;
            }

            if (resource.MonitorNewItems.HasValue)
            {
                game.MonitorNewItems = resource.MonitorNewItems.Value;
            }

            if (resource.QualityProfileId.HasValue)
            {
                game.QualityProfileId = resource.QualityProfileId.Value;
            }

            if (resource.SeriesType.HasValue)
            {
                game.SeriesType = resource.SeriesType.Value;
            }

            if (resource.PlatformFolder.HasValue)
            {
                game.PlatformFolder = resource.PlatformFolder.Value;
            }

            if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
            {
                game.RootFolderPath = resource.RootFolderPath;
                seriesToMove.Add(new BulkMoveGame
                {
                    GameId = game.Id,
                    SourcePath = game.Path
                });
            }

            if (resource.Tags != null)
            {
                var newTags = resource.Tags;
                var applyTags = resource.ApplyTags;

                switch (applyTags)
                {
                    case ApplyTags.Add:
                        newTags.ForEach(t => game.Tags.Add(t));
                        break;
                    case ApplyTags.Remove:
                        newTags.ForEach(t => game.Tags.Remove(t));
                        break;
                    case ApplyTags.Replace:
                        game.Tags = new HashSet<int>(newTags);
                        break;
                }
            }

            var validationResult = _seriesEditorValidator.Validate(game);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        if (resource.MoveFiles && seriesToMove.Any())
        {
            _commandQueueManager.Push(new BulkMoveGameCommand
            {
                DestinationRootFolder = resource.RootFolderPath,
                Game = seriesToMove
            });
        }

        return Accepted(_seriesService.UpdateSeries(gamesToUpdate, !resource.MoveFiles).ToResource());
    }

    [HttpDelete]
    public object DeleteGame([FromBody] GameEditorResource resource)
    {
        _seriesService.DeleteGame(resource.GameIds, resource.DeleteFiles, resource.AddImportListExclusion);

        return new { };
    }
}
