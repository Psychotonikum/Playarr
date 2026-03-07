using Playarr.Core.Validation;

namespace Playarr.Core.CustomFormats
{
    public abstract class CustomFormatSpecificationBase : ICustomFormatSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public virtual string InfoLink => "https://wiki.servarr.com/playarr/settings#custom-formats-2";

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }

        public ICustomFormatSpecification Clone()
        {
            return (ICustomFormatSpecification)MemberwiseClone();
        }

        public abstract PlayarrValidationResult Validate();

        public virtual bool IsSatisfiedBy(CustomFormatInput input)
        {
            var match = IsSatisfiedByWithoutNegate(input);

            if (Negate)
            {
                match = !match;
            }

            return match;
        }

        protected abstract bool IsSatisfiedByWithoutNegate(CustomFormatInput input);
    }
}
