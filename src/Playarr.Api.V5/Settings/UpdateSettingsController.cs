using FluentValidation;
using Playarr.Core.Configuration;
using Playarr.Core.Update;
using Playarr.Core.Validation.Paths;
using Playarr.Http;

namespace Playarr.Api.V5.Settings;

[V5ApiController("settings/update")]
public class UpdateSettingsController : SettingsController<UpdateSettingsResource>
{
    public UpdateSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        : base(configFileProvider, configService)
    {
        SharedValidator.RuleFor(c => c.UpdateScriptPath)
            .IsValidPath()
            .When(c => c.UpdateMechanism == UpdateMechanism.Script);
    }

    protected override UpdateSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return UpdateSettingsResourceMapper.ToResource(configFile);
    }
}
