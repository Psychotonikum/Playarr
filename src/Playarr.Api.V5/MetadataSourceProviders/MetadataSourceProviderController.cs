using Playarr.Core.MetadataSource.Providers;
using Playarr.SignalR;
using Playarr.Api.V5.Provider;
using Playarr.Http;

namespace Playarr.Api.V5.MetadataSourceProviders;

[V5ApiController]
public class MetadataSourceProviderController : ProviderControllerBase<MetadataSourceProviderResource, MetadataSourceProviderBulkResource, IMetadataSourceProvider, MetadataSourceDefinition>
{
    public static readonly MetadataSourceProviderResourceMapper ResourceMapper = new();
    public static readonly MetadataSourceProviderBulkResourceMapper BulkResourceMapper = new();

    public MetadataSourceProviderController(IBroadcastSignalRMessage signalRBroadcaster,
        MetadataSourceProviderFactory metadataSourceProviderFactory)
        : base(signalRBroadcaster, metadataSourceProviderFactory, "metadatasourceprovider", ResourceMapper, BulkResourceMapper)
    {
    }
}
