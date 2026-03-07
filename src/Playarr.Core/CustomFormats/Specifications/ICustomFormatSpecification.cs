using Playarr.Core.Validation;

namespace Playarr.Core.CustomFormats
{
    public interface ICustomFormatSpecification
    {
        int Order { get; }
        string InfoLink { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }

        PlayarrValidationResult Validate();

        ICustomFormatSpecification Clone();
        bool IsSatisfiedBy(CustomFormatInput input);
    }
}
