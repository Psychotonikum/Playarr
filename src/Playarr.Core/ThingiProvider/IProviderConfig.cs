using Playarr.Core.Validation;

namespace Playarr.Core.ThingiProvider
{
    public interface IProviderConfig
    {
        PlayarrValidationResult Validate();
    }
}
