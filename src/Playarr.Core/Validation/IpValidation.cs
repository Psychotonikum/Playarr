using FluentValidation;
using Playarr.Common.Extensions;

namespace Playarr.Core.Validation
{
    public static class IpValidation
    {
        public static IRuleBuilderOptions<T, string> ValidIpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x => x.IsValidIpAddress()).WithMessage("Must contain wildcard (*) or a valid IP Address");
        }
    }
}
