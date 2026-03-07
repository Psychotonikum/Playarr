using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;
using Playarr.Core.Validation.Paths;

namespace Playarr.Core.Download.Clients.Pneumatic
{
    public class PneumaticSettingsValidator : AbstractValidator<PneumaticSettings>
    {
        public PneumaticSettingsValidator()
        {
            RuleFor(c => c.NzbFolder).IsValidPath();
            RuleFor(c => c.StrmFolder).IsValidPath();
        }
    }

    public class PneumaticSettings : DownloadClientSettingsBase<PneumaticSettings>
    {
        private static readonly PneumaticSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "DownloadClientPneumaticSettingsNzbFolder", Type = FieldType.Path, HelpText = "DownloadClientPneumaticSettingsNzbFolderHelpText")]
        public string NzbFolder { get; set; }

        [FieldDefinition(1, Label = "DownloadClientPneumaticSettingsStrmFolder", Type = FieldType.Path, HelpText = "DownloadClientPneumaticSettingsStrmFolderHelpText")]
        public string StrmFolder { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
