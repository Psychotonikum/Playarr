using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class StatusSpecificationValidator : AbstractValidator<StatusSpecification>
    {
    }

    public class StatusSpecification : AutoTaggingSpecificationBase
    {
        private static readonly StatusSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Status";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationStatus", Type = FieldType.Select, SelectOptions = typeof(GameStatusType))]
        public int Status { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.Status == (GameStatusType)Status;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
