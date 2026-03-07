using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Simkl.User
{
    public class SimklUserSettingsValidator : SimklSettingsBaseValidator<SimklUserSettings>
    {
        public SimklUserSettingsValidator()
        {
            RuleFor(c => c.ListType).NotNull();
        }
    }

    public class SimklUserSettings : SimklSettingsBase<SimklUserSettings>
    {
        private static readonly SimklUserSettingsValidator Validator = new();

        public SimklUserSettings()
        {
            ListType = (int)SimklUserListType.Watching;
            ShowType = (int)SimklUserShowType.Shows;
        }

        [FieldDefinition(1, Label = "ImportListsSimklSettingsListType", Type = FieldType.Select, SelectOptions = typeof(SimklUserListType), HelpText = "ImportListsSimklSettingsListTypeHelpText")]
        public int ListType { get; set; }

        [FieldDefinition(1, Label = "ImportListsSimklSettingsShowType", Type = FieldType.Select, SelectOptions = typeof(SimklUserShowType), HelpText = "ImportListsSimklSettingsShowTypeHelpText")]
        public int ShowType { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
