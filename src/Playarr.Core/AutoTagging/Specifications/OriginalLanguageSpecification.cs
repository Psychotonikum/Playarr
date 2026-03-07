using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Languages;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class OriginalLanguageSpecificationValidator : AbstractValidator<OriginalLanguageSpecification>
    {
        public OriginalLanguageSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        }
    }

    public class OriginalLanguageSpecification : AutoTaggingSpecificationBase
    {
        private static readonly OriginalLanguageSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Original Language";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationOriginalLanguage", Type = FieldType.Select, SelectOptions = typeof(OriginalLanguageFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value == game.OriginalLanguage.Id;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
