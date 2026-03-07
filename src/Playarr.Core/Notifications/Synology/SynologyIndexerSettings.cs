using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.Synology
{
    public class SynologyIndexerSettingsValidator : AbstractValidator<SynologyIndexerSettings>
    {
    }

    public class SynologyIndexerSettings : NotificationSettingsBase<SynologyIndexerSettings>
    {
        private static readonly SynologyIndexerSettingsValidator Validator = new();

        public SynologyIndexerSettings()
        {
            UpdateLibrary = true;
        }

        [FieldDefinition(0, Label = "NotificationsSettingsUpdateLibrary", Type = FieldType.Checkbox, HelpText = "NotificationsSynologySettingsUpdateLibraryHelpText")]
        public bool UpdateLibrary { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
