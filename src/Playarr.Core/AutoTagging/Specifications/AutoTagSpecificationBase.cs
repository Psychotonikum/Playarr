using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public abstract class AutoTaggingSpecificationBase : IAutoTaggingSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }

        public IAutoTaggingSpecification Clone()
        {
            return (IAutoTaggingSpecification)MemberwiseClone();
        }

        public abstract PlayarrValidationResult Validate();

        public bool IsSatisfiedBy(Game game)
        {
            var match = IsSatisfiedByWithoutNegate(game);

            if (Negate)
            {
                match = !match;
            }

            return match;
        }

        protected abstract bool IsSatisfiedByWithoutNegate(Game game);
    }
}
