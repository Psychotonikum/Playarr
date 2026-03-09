using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.MetadataSource.Providers
{
    public class TinfoilProviderSettingsValidator : AbstractValidator<TinfoilProviderSettings>
    {
    }

    public class TinfoilProviderSettings : IProviderConfig
    {
        private static readonly TinfoilProviderSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Cache Duration (Hours)", Type = FieldType.Number, HelpText = "How long to cache the title database (default: 12 hours)")]
        public int CacheDurationHours { get; set; } = 12;

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }

    public class TinfoilProvider : MetadataSourceProviderBase<TinfoilProviderSettings>
    {
        public TinfoilProvider(NLog.Logger logger)
            : base(logger)
        {
        }

        public override string Name => "Tinfoil (Switch)";
        public override bool SupportsSearch => true;
        public override bool SupportsCalendar => false;
        public override bool SupportsMetadataDownload => true;
    }
}
