using System;
using System.Collections.Generic;
using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Playarr
{
    public class PlayarrSettingsValidator : AbstractValidator<PlayarrSettings>
    {
        public PlayarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class PlayarrSettings : ImportListSettingsBase<PlayarrSettings>
    {
        private static readonly PlayarrSettingsValidator Validator = new();

        public PlayarrSettings()
        {
            ApiKey = "";
            ProfileIds = Array.Empty<int>();
            LanguageProfileIds = Array.Empty<int>();
            TagIds = Array.Empty<int>();
            RootFolderPaths = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "ImportListsPlayarrSettingsFullUrl", HelpText = "ImportListsPlayarrSettingsFullUrlHelpText")]
        public override string BaseUrl { get; set; } = string.Empty;

        [FieldDefinition(1, Label = "ApiKey", HelpText = "ImportListsPlayarrSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "ImportListsPlayarrSettingsSyncSeasonMonitoring", HelpText = "ImportListsPlayarrSettingsSyncSeasonMonitoringHelpText", Type = FieldType.Checkbox)]
        public bool SyncSeasonMonitoring { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getProfiles", Label = "QualityProfiles", HelpText = "ImportListsPlayarrSettingsQualityProfilesHelpText")]
        public IEnumerable<int> ProfileIds { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptionsProviderAction = "getTags", Label = "Tags", HelpText = "ImportListsPlayarrSettingsTagsHelpText")]
        public IEnumerable<int> TagIds { get; set; }

        [FieldDefinition(5, Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", Label = "RootFolders", HelpText = "ImportListsPlayarrSettingsRootFoldersHelpText")]
        public IEnumerable<string> RootFolderPaths { get; set; }

        // TODO: Remove this eventually, no translation added as deprecated
        [FieldDefinition(6, Type = FieldType.Select, SelectOptionsProviderAction = "getLanguageProfiles", Label = "Language Profiles", HelpText = "Language Profiles from the source instance to import from")]
        public IEnumerable<int> LanguageProfileIds { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
