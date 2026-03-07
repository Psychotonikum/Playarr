using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class TagSpecificationValidator : AbstractValidator<TagSpecification>
    {
        public TagSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class TagSpecification : AutoTaggingSpecificationBase
    {
        private static readonly TagSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Tag";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationTag", Type = FieldType.SeriesTag)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.Tags.Contains(Value);
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
