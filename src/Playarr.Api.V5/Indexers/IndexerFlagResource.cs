using Newtonsoft.Json;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Indexers;

public class IndexerFlagResource : RestResource
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public new int Id { get; set; }
    public string? Name { get; set; }
    public string? NameLower => Name?.ToLowerInvariant();
}
