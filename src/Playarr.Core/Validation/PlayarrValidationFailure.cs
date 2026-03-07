using FluentValidation.Results;

namespace Playarr.Core.Validation
{
    public class PlayarrValidationFailure : ValidationFailure
    {
        public bool IsWarning { get; set; }
        public string DetailedDescription { get; set; }
        public string InfoLink { get; set; }

        public PlayarrValidationFailure(string propertyName, string error)
            : base(propertyName, error)
        {
        }

        public PlayarrValidationFailure(string propertyName, string error, object attemptedValue)
            : base(propertyName, error, attemptedValue)
        {
        }

        public PlayarrValidationFailure(ValidationFailure validationFailure)
            : base(validationFailure.PropertyName, validationFailure.ErrorMessage, validationFailure.AttemptedValue)
        {
            CustomState = validationFailure.CustomState;
            var state = validationFailure.CustomState as PlayarrValidationState;

            IsWarning = state is { IsWarning: true };
        }
    }
}
