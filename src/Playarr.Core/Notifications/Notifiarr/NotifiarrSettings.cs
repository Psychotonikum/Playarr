using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.Notifiarr
{
    public class NotifiarrSettingsValidator : AbstractValidator<NotifiarrSettings>
    {
        public NotifiarrSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class NotifiarrSettings : NotificationSettingsBase<NotifiarrSettings>
    {
        private static readonly NotifiarrSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsNotifiarrSettingsApiKeyHelpText", HelpLink = "https://notifiarr.com")]
        public string ApiKey { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
