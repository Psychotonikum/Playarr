using FluentValidation;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class MonitoredSpecificationValidator : AbstractValidator<MonitoredSpecification>
    {
    }

    public class MonitoredSpecification : AutoTaggingSpecificationBase
    {
        private static readonly MonitoredSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Monitored";

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.Monitored;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
