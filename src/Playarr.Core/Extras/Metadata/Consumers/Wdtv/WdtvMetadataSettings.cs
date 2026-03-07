using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Extras.Metadata.Consumers.Wdtv
{
    public class WdtvSettingsValidator : AbstractValidator<WdtvMetadataSettings>
    {
    }

    public class WdtvMetadataSettings : IProviderConfig
    {
        private static readonly WdtvSettingsValidator Validator = new WdtvSettingsValidator();

        public WdtvMetadataSettings()
        {
            EpisodeMetadata = true;
            GameImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsEpisodeMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Platform##\\filename.xml")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "folder.jpg")]
        public bool GameImages { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Platform##\\folder.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Platform##\\filename.metathumb")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
