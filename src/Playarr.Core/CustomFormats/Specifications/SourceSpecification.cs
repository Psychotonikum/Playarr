using FluentValidation;
using Playarr.Core.Annotations;
using Playarr.Core.Qualities;
using Playarr.Core.Validation;

namespace Playarr.Core.CustomFormats
{
    public class SourceSpecificationValidator : AbstractValidator<SourceSpecification>
    {
        public SourceSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class SourceSpecification : CustomFormatSpecificationBase
    {
        private static readonly SourceSpecificationValidator Validator = new SourceSpecificationValidator();

        public override int Order => 5;
        public override string ImplementationName => "Source";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationSource", Type = FieldType.Select, SelectOptions = typeof(QualitySource))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.RomInfo?.Quality?.Quality?.Source ?? (int)QualitySource.Unknown) == (QualitySource)Value;
        }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate(this));
        }
    }
}
