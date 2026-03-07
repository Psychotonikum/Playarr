using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class OriginalCountrySpecificationValidator : AbstractValidator<OriginalCountrySpecification>
    {
        public OriginalCountrySpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();

            RuleFor(c => c.Value).Custom((countries, context) =>
            {
                if (countries.Any(c => c.Length != 3))
                {
                    context.AddFailure("Country code must be 3 characters long");
                }
            });
        }
    }

    public class OriginalCountrySpecification : AutoTaggingSpecificationBase
    {
        private static readonly OriginalCountrySpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Original Country";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationOriginalCountry", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return Value.Any(network => game.OriginalCountry.EqualsIgnoreCase(network));
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
