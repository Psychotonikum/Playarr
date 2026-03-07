using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Games;
using Playarr.Core.Validation;
using Playarr.Core.Validation.Paths;

namespace Playarr.Core.AutoTagging.Specifications
{
    public class RootFolderSpecificationValidator : AbstractValidator<RootFolderSpecification>
    {
        public RootFolderSpecificationValidator()
        {
            RuleFor(c => c.Value).IsValidPath();
        }
    }

    public class RootFolderSpecification : AutoTaggingSpecificationBase
    {
        private static readonly RootFolderSpecificationValidator Validator = new RootFolderSpecificationValidator();

        public override int Order => 1;
        public override string ImplementationName => "Root Folder";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationRootFolder", Type = FieldType.RootFolder)]
        public string Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Game game)
        {
            return game.RootFolderPath.PathEquals(Value);
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
