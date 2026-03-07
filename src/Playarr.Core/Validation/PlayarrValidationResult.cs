using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Playarr.Common.Extensions;

namespace Playarr.Core.Validation
{
    public class PlayarrValidationResult : ValidationResult
    {
        public PlayarrValidationResult()
        {
            Failures = new List<PlayarrValidationFailure>();
            Errors = new List<PlayarrValidationFailure>();
            Warnings = new List<PlayarrValidationFailure>();
        }

        public PlayarrValidationResult(ValidationResult validationResult)
            : this(validationResult.Errors)
        {
        }

        public PlayarrValidationResult(IEnumerable<ValidationFailure> failures)
        {
            var errors = new List<PlayarrValidationFailure>();
            var warnings = new List<PlayarrValidationFailure>();

            foreach (var failureBase in failures)
            {
                if (failureBase is not PlayarrValidationFailure failure)
                {
                    failure = new PlayarrValidationFailure(failureBase);
                }

                if (failure.IsWarning)
                {
                    warnings.Add(failure);
                }
                else
                {
                    errors.Add(failure);
                }
            }

            Failures = errors.Concat(warnings).ToList();
            Errors = errors;
            errors.ForEach(base.Errors.Add);
            Warnings = warnings;
        }

        public IList<PlayarrValidationFailure> Failures { get; private set; }
        public new IList<PlayarrValidationFailure> Errors { get; private set; }
        public IList<PlayarrValidationFailure> Warnings { get; private set; }

        public virtual bool HasWarnings => Warnings.Any();

        public override bool IsValid => Errors.Empty();
    }
}
