using Playarr.Core.Validation;

namespace Playarr.Core.ThingiProvider
{
    public class NullConfig : IProviderConfig
    {
        public static readonly NullConfig Instance = new NullConfig();

        public PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult();
        }
    }
}
