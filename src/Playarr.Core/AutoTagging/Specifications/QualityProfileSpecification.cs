using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class QualityProfileSpecificationValidator : AbstractValidator<QualityProfileSpecification>
    {
        public QualityProfileSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class QualityProfileSpecification : AutoTaggingSpecificationBase
    {
        private static readonly QualityProfileSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Quality Profile";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationQualityProfile", Type = FieldType.QualityProfile)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value == game.QualityProfileId;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
