using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Extras.Metadata.Consumers.Batocera
{
    public class BatoceraMetadataSettingsValidator : AbstractValidator<BatoceraMetadataSettings>
    {
    }

    public class BatoceraMetadataSettings : IProviderConfig
    {
        private static readonly BatoceraMetadataSettingsValidator Validator = new BatoceraMetadataSettingsValidator();

        public BatoceraMetadataSettings()
        {
            GamelistXml = true;
            GameImages = true;
        }

        [FieldDefinition(0, Label = "MetadataBatoceraSettingsGamelistXml", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataBatoceraSettingsGamelistXmlHelpText")]
        public bool GamelistXml { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "MetadataBatoceraSettingsGameImagesHelpText")]
        public bool GameImages { get; set; }

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
