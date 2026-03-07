using System.Text.RegularExpressions;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Download.Clients.Tribler
{
    public class TriblerSettingsValidator : AbstractValidator<TriblerDownloadSettings>
    {
        public TriblerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.GameCategory).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters a-z and -");
            RuleFor(c => c.GameCategory).Empty()
                .When(c => c.GameDirectory.IsNotNullOrWhiteSpace())
                .WithMessage("Cannot use Category and Directory");
            RuleFor(c => c.AnonymityLevel).GreaterThanOrEqualTo(0);
        }
    }

    public class TriblerDownloadSettings : IProviderConfig
    {
        private static readonly TriblerSettingsValidator Validator = new TriblerSettingsValidator();

        public TriblerDownloadSettings()
        {
            Host = "localhost";
            Port = 20100;
            UrlBase = "";
            AnonymityLevel = 1;
            SafeSeeding = true;
        }

        [FieldDefinition(1, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(2, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(3, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        public bool UseSsl { get; set; }

        [FieldDefinition(4, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "Tribler")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]")]
        public string UrlBase { get; set; }

        [FieldDefinition(5, Label = "ApiKey", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "DownloadClientTriblerSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string GameCategory { get; set; }

        [FieldDefinition(7, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientTriblerSettingsDirectoryHelpText")]
        public string GameDirectory { get; set; }

        [FieldDefinition(8, Label = "DownloadClientTriblerSettingsAnonymityLevel", Type = FieldType.Number, HelpText = "DownloadClientTriblerSettingsAnonymityLevelHelpText")]
        [FieldToken(TokenField.HelpText, "DownloadClientTriblerSettingsAnonymityLevel", "url", "https://www.tribler.org/anonymity.html")]
        public int AnonymityLevel { get; set; }

        [FieldDefinition(9, Label = "DownloadClientTriblerSettingsSafeSeeding", Type = FieldType.Checkbox, HelpText = "DownloadClientTriblerSettingsSafeSeedingHelpText")]
        public bool SafeSeeding { get; set; }

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
