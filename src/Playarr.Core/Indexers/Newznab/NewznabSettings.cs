using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Equ;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Languages;
using Playarr.Core.Validation;

namespace Playarr.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyAllowList =
        {
            "nzbs.org",
            "nzb.life",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            return settings.BaseUrl != null && ApiKeyAllowList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.Categories.Empty())
                {
                    context.AddFailure("'Categories' must be provided");
                }
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : PropertywiseEquatable<NewznabSettings>, IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new();

        public NewznabSettings()
        {
            ApiPath = "/api";
            Categories = new[] { 1000 };
            MultiLanguages = Array.Empty<int>();
            FailDownloads = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsApiPath", HelpText = "IndexerSettingsApiPathHelpText", Advanced = true)]
        [FieldToken(TokenField.HelpText, "IndexerSettingsApiPath", "url", "/api")]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "IndexerSettingsCategories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "IndexerSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        // Kept for backward compatibility with existing DB entries (no UI field)
        public IEnumerable<int> AnimeCategories { get; set; } = Enumerable.Empty<int>();

        // Legacy property for backward compatibility
        public bool AnimeStandardFormatSearch { get; set; }

        [FieldDefinition(4, Label = "IndexerSettingsAdditionalParameters", HelpText = "IndexerSettingsAdditionalNewznabParametersHelpText", Advanced = true)]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(5, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(6, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "IndexerSettingsFailDownloads", HelpText = "IndexerSettingsFailDownloadsHelpText", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        // Field 6 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
