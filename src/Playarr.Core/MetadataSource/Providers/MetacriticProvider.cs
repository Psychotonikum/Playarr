using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.MetadataSource.Providers
{
    public class MetacriticProviderSettingsValidator : AbstractValidator<MetacriticProviderSettings>
    {
    }

    public class MetacriticProviderSettings : IProviderConfig
    {
        private static readonly MetacriticProviderSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Use as Rating Source", Type = FieldType.Checkbox, HelpText = "Use Metacritic scores instead of IGDB ratings")]
        public bool UseAsRatingSource { get; set; }

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }

    public class MetacriticProvider : MetadataSourceProviderBase<MetacriticProviderSettings>
    {
        public MetacriticProvider(NLog.Logger logger)
            : base(logger)
        {
        }

        public override string Name => "Metacritic";
        public override bool SupportsSearch => false;
        public override bool SupportsCalendar => true;
        public override bool SupportsMetadataDownload => false;
    }
}
