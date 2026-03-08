using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Datastore.Events;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;
using Playarr.Core.Messaging.Events;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;

namespace Playarr.Api.V5.GameSystems;

[V5ApiController]
public class GameSystemController : RestControllerWithSignalR<GameSystemResource, GameSystem>,
                                    IHandle<GameSystemUpdatedEvent>
{
    private readonly IGameSystemService _gameSystemService;

    public GameSystemController(IBroadcastSignalRMessage signalRBroadcaster,
                                IGameSystemService gameSystemService)
        : base(signalRBroadcaster)
    {
        _gameSystemService = gameSystemService;

        SharedValidator.RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("System name is required");

        SharedValidator.RuleFor(s => s.FolderName)
            .NotEmpty()
            .WithMessage("Folder name is required")
            .Matches("^[a-z0-9_-]+$")
            .WithMessage("Folder name must be lowercase alphanumeric with optional hyphens or underscores");
    }

    protected override GameSystemResource GetResourceById(int id)
    {
        return _gameSystemService.Get(id).ToResource();
    }

    [HttpGet]
    [Produces("application/json")]
    public List<GameSystemResource> GetAll()
    {
        return _gameSystemService.All().ToResource();
    }

    [RestPostById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameSystemResource> Create([FromBody] GameSystemResource resource)
    {
        var model = resource.ToModel();
        var created = _gameSystemService.Add(model);

        return Created(created.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public ActionResult<GameSystemResource> Update([FromBody] GameSystemResource resource)
    {
        var model = resource.ToModel();
        _gameSystemService.Update(model);

        return Accepted(resource.Id);
    }

    [RestDeleteById]
    public ActionResult Delete(int id)
    {
        _gameSystemService.Delete(id);

        return NoContent();
    }

    [NonAction]
    public void Handle(GameSystemUpdatedEvent message)
    {
        BroadcastResourceChange(ModelAction.Sync);
    }
}
