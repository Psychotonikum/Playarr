using Playarr.Core.Configuration;
using Playarr.Http;

namespace Playarr.Api.V3.Config
{
    [V3ApiController("config/metadatasource")]
    public class MetadataSourceConfigController : ConfigController<MetadataSourceConfigResource>
    {
        public MetadataSourceConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override MetadataSourceConfigResource ToResource(IConfigService model)
        {
            return MetadataSourceConfigResourceMapper.ToResource(model);
        }
    }
}
