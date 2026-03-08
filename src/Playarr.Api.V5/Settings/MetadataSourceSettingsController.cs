using Playarr.Core.Configuration;
using Playarr.Http;

namespace Playarr.Api.V5.Settings;

[V5ApiController("settings/metadatasource")]
public class MetadataSourceSettingsController : SettingsController<MetadataSourceSettingsResource>
{
    public MetadataSourceSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        : base(configFileProvider, configService)
    {
    }

    protected override MetadataSourceSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return MetadataSourceSettingsResourceMapper.ToResource(model);
    }
}
