using System;
using System.Collections.Generic;
using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.SendGrid
{
    public class SendGridSettingsValidator : AbstractValidator<SendGridSettings>
    {
        public SendGridSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.From).NotEmpty().EmailAddress();
            RuleFor(c => c.Recipients).NotEmpty();
            RuleForEach(c => c.Recipients).NotEmpty().EmailAddress();
        }
    }

    public class SendGridSettings : NotificationSettingsBase<SendGridSettings>
    {
        private static readonly SendGridSettingsValidator Validator = new();

        public SendGridSettings()
        {
            BaseUrl = "https://api.sendgrid.com/v3/";
            Recipients = Array.Empty<string>();
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "ApiKey", HelpText = "NotificationsSendGridSettingsApiKeyHelpText", HelpLink = "https://sendgrid.com/docs/ui/account-and-settings/api-keys/#creating-an-api-key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "NotificationsEmailSettingsFromAddress")]
        public string From { get; set; }

        [FieldDefinition(3, Label = "NotificationsEmailSettingsRecipientAddress", Type = FieldType.Tag)]
        public IEnumerable<string> Recipients { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
