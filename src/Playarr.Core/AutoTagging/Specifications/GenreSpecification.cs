using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class GenreSpecificationValidator : AbstractValidator<GenreSpecification>
    {
        public GenreSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class GenreSpecification : AutoTaggingSpecificationBase
    {
        private static readonly GenreSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Genre";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationGenre", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.Genres.Any(genre => Value.ContainsIgnoreCase(genre));
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
