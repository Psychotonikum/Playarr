using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.CustomFormats
{
    public class SizeSpecificationValidator : AbstractValidator<SizeSpecification>
    {
        public SizeSpecificationValidator()
        {
            RuleFor(c => c.Min).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Max).GreaterThan(c => c.Min);
            RuleFor(c => c.Max).LessThanOrEqualTo(double.MaxValue);
        }
    }

    public class SizeSpecification : CustomFormatSpecificationBase
    {
        private static readonly SizeSpecificationValidator Validator = new SizeSpecificationValidator();

        public override int Order => 8;
        public override string ImplementationName => "Size";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationMinimumSize", HelpText = "CustomFormatsSpecificationMinimumSizeHelpText", Unit = "GB", Type = FieldType.Number)]
        public double Min { get; set; }

        [FieldDefinition(1, Label = "CustomFormatsSpecificationMaximumSize", HelpText = "CustomFormatsSpecificationMaximumSizeHelpText", Unit = "GB", Type = FieldType.Number)]
        public double Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var size = input.Size;

            return size > Min.Gigabytes() && size <= Max.Gigabytes();
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
