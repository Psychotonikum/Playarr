using Playarr.Common.Exceptions;

namespace Playarr.Core.ThingiProvider
{
    public class ConfigContractNotFoundException : PlayarrException
    {
        public ConfigContractNotFoundException(string contract)
            : base("Couldn't find config contract " + contract)
        {
        }
    }
}
