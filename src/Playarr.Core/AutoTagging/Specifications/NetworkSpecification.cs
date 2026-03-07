using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class NetworkSpecificationValidator : AbstractValidator<NetworkSpecification>
    {
        public NetworkSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class NetworkSpecification : AutoTaggingSpecificationBase
    {
        private static readonly NetworkSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Network";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationNetwork", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value.Any(network => game.Network.EqualsIgnoreCase(network));
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
