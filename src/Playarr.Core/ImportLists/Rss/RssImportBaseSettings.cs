using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Rss
{
    public class RssImportSettingsValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : RssImportBaseSettings<TSettings>
    {
        public RssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class RssImportBaseSettings<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : RssImportBaseSettings<TSettings>
    {
        private static readonly RssImportSettingsValidator<TSettings> Validator = new();

        public override string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRssUrl", Type = FieldType.Textbox)]
        public virtual string Url { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this as TSettings));
        }
    }
}
