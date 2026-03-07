using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Playarr.Core.Validation
{
    public static class PlayarrValidationExtensions
    {
        public static PlayarrValidationResult Filter(this PlayarrValidationResult result, params string[] fields)
        {
            var failures = result.Failures.Where(v => fields.Contains(v.PropertyName)).ToArray();

            return new PlayarrValidationResult(failures);
        }

        public static void ThrowOnError(this PlayarrValidationResult result)
        {
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static bool HasErrors(this List<ValidationFailure> list)
        {
            return list.Any(item => item is not PlayarrValidationFailure { IsWarning: true });
        }
    }
}
