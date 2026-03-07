using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Validation;

namespace Playarr.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcMetadataSettings>
    {
    }

    public class XbmcMetadataSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcMetadataSettings()
        {
            SeriesMetadata = true;
            SeriesMetadataEpisodeGuide = false;
            SeriesMetadataUrl = false;
            EpisodeMetadata = true;
            GameImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsSeriesMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataHelpText")]
        public bool SeriesMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsSeriesMetadataEpisodeGuide", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataEpisodeGuideHelpText", Advanced = true)]
        public bool SeriesMetadataEpisodeGuide { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsSeriesMetadataUrl", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataUrlHelpText", Advanced = true)]
        public bool SeriesMetadataUrl { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsEpisodeMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "<filename>.nfo")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(4, Label = "MetadataSettingsEpisodeMetadataImageThumbs", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "MetadataXmbcSettingsEpisodeMetadataImageThumbsHelpText", Advanced = true)]
        public bool EpisodeImageThumb { get; set; }

        [FieldDefinition(5, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "fanart.jpg, poster.jpg, banner.jpg")]
        public bool GameImages { get; set; }

        [FieldDefinition(6, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "platform##-poster.jpg, platform##-banner.jpg, platform-specials-poster.jpg, platform-specials-banner.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(7, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "<filename>-thumb.jpg")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
