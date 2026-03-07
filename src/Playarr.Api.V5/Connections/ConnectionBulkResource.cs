using Playarr.Core.Notifications;
using Playarr.Api.V5.Provider;

namespace Playarr.Api.V5.Connections;

public class ConnectionBulkResource : ProviderBulkResource<ConnectionBulkResource>
{
}

public class ConnectionBulkResourceMapper : ProviderBulkResourceMapper<ConnectionBulkResource, NotificationDefinition>
{
}
