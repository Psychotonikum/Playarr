using FluentValidation;

using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Custom
{
    public class CustomSettingsValidator : AbstractValidator<CustomSettings>
    {
        public CustomSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class CustomSettings : ImportListSettingsBase<CustomSettings>
    {
        private static readonly CustomSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "ImportListsCustomListSettingsUrl", HelpText = "ImportListsCustomListSettingsUrlHelpText")]
        public override string BaseUrl { get; set; } = string.Empty;

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
