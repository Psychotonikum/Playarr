using System.Collections.Generic;
using FluentValidation.Validators;
using Playarr.Common.Extensions;

namespace Playarr.Http.Validation
{
    public class EmptyCollectionValidator<T> : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Collection Must Be Empty";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return context.PropertyValue is IEnumerable<T> collection && collection.Empty();
        }
    }
}
