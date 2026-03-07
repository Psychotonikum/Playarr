using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxSettingsValidator : AbstractValidator<RoksboxMetadataSettings>
    {
    }

    public class RoksboxMetadataSettings : IProviderConfig
    {
        private static readonly RoksboxSettingsValidator Validator = new RoksboxSettingsValidator();

        public RoksboxMetadataSettings()
        {
            EpisodeMetadata = true;
            GameImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsEpisodeMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Platform##\\filename.xml")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Game Title.jpg")]
        public bool GameImages { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Platform ##.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Platform##\\filename.jpg")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
