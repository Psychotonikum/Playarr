using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.AutoTagging.Specifications
{
    public interface IAutoTaggingSpecification
    {
        int Order { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }
        PlayarrValidationResult Validate();

        IAutoTaggingSpecification Clone();
        bool IsSatisfiedBy(Game game);
    }
}
