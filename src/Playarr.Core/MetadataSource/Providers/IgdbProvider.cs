using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.MetadataSource.Providers
{
    public class IgdbProviderSettingsValidator : AbstractValidator<IgdbProviderSettings>
    {
        public IgdbProviderSettingsValidator()
        {
            RuleFor(c => c.TwitchClientId).NotEmpty();
            RuleFor(c => c.TwitchClientSecret).NotEmpty();
        }
    }

    public class IgdbProviderSettings : IProviderConfig
    {
        private static readonly IgdbProviderSettingsValidator Validator = new();

        public IgdbProviderSettings()
        {
            TwitchClientId = string.Empty;
            TwitchClientSecret = string.Empty;
        }

        [FieldDefinition(0, Label = "Twitch Client ID", Type = FieldType.Textbox, HelpText = "Client ID from your Twitch Developer Application (required for IGDB)", HelpLink = "https://dev.twitch.tv/console/apps")]
        public string TwitchClientId { get; set; }

        [FieldDefinition(1, Label = "Twitch Client Secret", Type = FieldType.Password, HelpText = "Client Secret from your Twitch Developer Application")]
        public string TwitchClientSecret { get; set; }

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }

    public class IgdbProvider : MetadataSourceProviderBase<IgdbProviderSettings>
    {
        public IgdbProvider(NLog.Logger logger)
            : base(logger)
        {
        }

        public override string Name => "IGDB";
        public override bool SupportsSearch => true;
        public override bool SupportsCalendar => true;
        public override bool SupportsMetadataDownload => true;
    }
}
