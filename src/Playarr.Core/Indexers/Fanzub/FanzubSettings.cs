using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Languages;
using Playarr.Core.Validation;

namespace Playarr.Core.Indexers.Fanzub
{
    public class FanzubSettingsValidator : AbstractValidator<FanzubSettings>
    {
        public FanzubSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class FanzubSettings : PropertywiseEquatable<FanzubSettings>, IIndexerSettings
    {
        private static readonly FanzubSettingsValidator Validator = new();

        public FanzubSettings()
        {
            BaseUrl = "http://fanzub.com/rss/";
            MultiLanguages = Array.Empty<int>();
            FailDownloads = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "IndexerSettingsRssUrl", HelpText = "IndexerSettingsRssUrlHelpText")]
        [FieldToken(TokenField.HelpText, "IndexerSettingsRssUrl", "indexer", "Fanzub")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsAnimeStandardFormatSearch", Type = FieldType.Checkbox, HelpText = "IndexerSettingsAnimeStandardFormatSearchHelpText")]
        public bool AnimeStandardFormatSearch { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "IndexerSettingsFailDownloads", HelpText = "IndexerSettingsFailDownloadsHelpText", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
