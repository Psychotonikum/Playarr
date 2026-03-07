using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Extras.Metadata;
using Playarr.SignalR;
using Playarr.Api.V5.Provider;
using Playarr.Http;

namespace Playarr.Api.V5.Metadata;

[V5ApiController]
public class MetadataController : ProviderControllerBase<MetadataResource, MetadataBulkResource, IMetadata, MetadataDefinition>
{
    public static readonly MetadataResourceMapper ResourceMapper = new();
    public static readonly MetadataBulkResourceMapper BulkResourceMapper = new();

    public MetadataController(IBroadcastSignalRMessage signalRBroadcaster, IMetadataFactory metadataFactory)
        : base(signalRBroadcaster, metadataFactory, "metadata", ResourceMapper, BulkResourceMapper)
    {
    }

    [NonAction]
    public override ActionResult<MetadataResource> UpdateProvider([FromBody] MetadataBulkResource providerResource)
    {
        throw new NotImplementedException();
    }

    [NonAction]
    public override ActionResult DeleteProviders([FromBody] MetadataBulkResource resource)
    {
        throw new NotImplementedException();
    }
}
