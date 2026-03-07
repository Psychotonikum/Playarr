using System.Text.Json.Serialization;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Localization;

public class LanguageResource : RestResource
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public new int Id { get; set; }
    public string? Name { get; set; }
    public string? NameLower => Name?.ToLowerInvariant();
}
