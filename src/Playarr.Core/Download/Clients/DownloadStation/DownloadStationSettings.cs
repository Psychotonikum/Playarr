using System.Text.RegularExpressions;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.Download.Clients.DownloadStation
{
    public class DownloadStationSettingsValidator : AbstractValidator<DownloadStationSettings>
    {
        public DownloadStationSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.GameDirectory).Matches(@"^(?!/).+")
                                       .When(c => c.GameDirectory.IsNotNullOrWhiteSpace())
                                       .WithMessage("Cannot start with /");

            RuleFor(c => c.GameCategory).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters a-z and -");

            RuleFor(c => c.GameCategory).Empty()
                                      .When(c => c.GameDirectory.IsNotNullOrWhiteSpace())
                                      .WithMessage("Cannot use Category and Directory");
        }
    }

    public class DownloadStationSettings : DownloadClientSettingsBase<DownloadStationSettings>
    {
        private static readonly DownloadStationSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "Download Station")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategorySubFolderHelpText")]
        public string GameCategory { get; set; }

        [FieldDefinition(6, Label = "Directory", Type = FieldType.Textbox, HelpText = "DownloadClientDownloadStationSettingsDirectoryHelpText")]
        public string GameDirectory { get; set; }

        public DownloadStationSettings()
        {
            this.Host = "127.0.0.1";
            this.Port = 5000;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
