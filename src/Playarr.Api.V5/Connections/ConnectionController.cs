using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Notifications;
using Playarr.SignalR;
using Playarr.Api.V5.Provider;
using Playarr.Http;

namespace Playarr.Api.V5.Connections;

[V5ApiController]
public class ConnectionController : ProviderControllerBase<ConnectionResource, ConnectionBulkResource, INotification, NotificationDefinition>
{
    public static readonly ConnectionResourceMapper ResourceMapper = new();
    public static readonly ConnectionBulkResourceMapper BulkResourceMapper = new();

    public ConnectionController(IBroadcastSignalRMessage signalRBroadcaster, NotificationFactory notificationFactory)
        : base(signalRBroadcaster, notificationFactory, "connection", ResourceMapper, BulkResourceMapper)
    {
    }

    [NonAction]
    public override ActionResult<ConnectionResource> UpdateProvider([FromBody] ConnectionBulkResource providerResource)
    {
        throw new NotImplementedException();
    }

    [NonAction]
    public override ActionResult DeleteProviders([FromBody] ConnectionBulkResource resource)
    {
        throw new NotImplementedException();
    }
}
