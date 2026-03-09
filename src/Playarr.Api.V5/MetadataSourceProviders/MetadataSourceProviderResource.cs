using Playarr.Core.MetadataSource.Providers;
using Playarr.Api.V5.Provider;

namespace Playarr.Api.V5.MetadataSourceProviders;

public class MetadataSourceProviderResource : ProviderResource<MetadataSourceProviderResource>
{
    public bool EnableSearch { get; set; }
    public bool EnableCalendar { get; set; }
    public bool DownloadMetadata { get; set; }
    public bool SupportsSearch { get; set; }
    public bool SupportsCalendar { get; set; }
    public bool SupportsMetadataDownload { get; set; }
}

public class MetadataSourceProviderResourceMapper : ProviderResourceMapper<MetadataSourceProviderResource, MetadataSourceDefinition>
{
    public override MetadataSourceProviderResource ToResource(MetadataSourceDefinition definition)
    {
        var resource = base.ToResource(definition);

        resource.EnableSearch = definition.EnableSearch;
        resource.EnableCalendar = definition.EnableCalendar;
        resource.DownloadMetadata = definition.DownloadMetadata;
        resource.SupportsSearch = definition.SupportsSearch;
        resource.SupportsCalendar = definition.SupportsCalendar;
        resource.SupportsMetadataDownload = definition.SupportsMetadataDownload;

        return resource;
    }

    public override MetadataSourceDefinition ToModel(MetadataSourceProviderResource resource, MetadataSourceDefinition? existingDefinition)
    {
        var definition = base.ToModel(resource, existingDefinition);

        definition.EnableSearch = resource.EnableSearch;
        definition.EnableCalendar = resource.EnableCalendar;
        definition.DownloadMetadata = resource.DownloadMetadata;

        return definition;
    }
}

public class MetadataSourceProviderBulkResource : ProviderBulkResource<MetadataSourceProviderBulkResource>
{
}

public class MetadataSourceProviderBulkResourceMapper : ProviderBulkResourceMapper<MetadataSourceProviderBulkResource, MetadataSourceDefinition>
{
}
