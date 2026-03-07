using System.Linq;
using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Languages;
using Playarr.Core.Validation;

namespace Playarr.Core.CustomFormats
{
    public class LanguageSpecificationValidator : AbstractValidator<LanguageSpecification>
    {
        public LanguageSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Language.All.Any(o => o.Id == value))
                {
                    context.AddFailure(string.Format("Invalid Language condition value: {0}", value));
                }
            });
        }
    }

    public class LanguageSpecification : CustomFormatSpecificationBase
    {
        private static readonly LanguageSpecificationValidator Validator = new LanguageSpecificationValidator();

        public override int Order => 3;
        public override string ImplementationName => "Language";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationLanguage", Type = FieldType.Select, SelectOptions = typeof(LanguageFieldConverter))]
        public int Value { get; set; }

        [FieldDefinition(1, Label = "CustomFormatsSpecificationExceptLanguage", HelpText = "CustomFormatsSpecificationExceptLanguageHelpText", Type = FieldType.Checkbox)]
        public bool ExceptLanguage { get; set; }

        public override bool IsSatisfiedBy(CustomFormatInput input)
        {
            if (Negate)
            {
                return IsSatisfiedByWithNegate(input);
            }

            return IsSatisfiedByWithoutNegate(input);
        }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var comparedLanguage = input.RomInfo != null && input.Game != null && Value == Language.Original.Id && input.Game.OriginalLanguage != Language.Unknown
                ? input.Game.OriginalLanguage
                : (Language)Value;

            if (ExceptLanguage)
            {
                return input.Languages?.Any(l => l != comparedLanguage) ?? false;
            }

            return input.Languages?.Contains(comparedLanguage) ?? false;
        }

        private bool IsSatisfiedByWithNegate(CustomFormatInput input)
        {
            var comparedLanguage = input.RomInfo != null && input.Game != null && Value == Language.Original.Id && input.Game.OriginalLanguage != Language.Unknown
                ? input.Game.OriginalLanguage
                : (Language)Value;

            if (ExceptLanguage)
            {
                return !input.Languages?.Any(l => l != comparedLanguage) ?? false;
            }

            return !input.Languages?.Contains(comparedLanguage) ?? false;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
