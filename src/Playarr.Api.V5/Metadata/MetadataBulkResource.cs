using Playarr.Core.Extras.Metadata;
using Playarr.Api.V5.Provider;

namespace Playarr.Api.V5.Metadata;

public class MetadataBulkResource : ProviderBulkResource<MetadataBulkResource>
{
}

public class MetadataBulkResourceMapper : ProviderBulkResourceMapper<MetadataBulkResource, MetadataDefinition>
{
}
