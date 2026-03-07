using FluentValidation;
using FluentValidation.Validators;
using Playarr.Common.Extensions;

namespace Playarr.Core.Validation
{
    public static class UrlValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new UrlValidator());
        }
    }

    public class UrlValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid Url: '{url}'";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            context.MessageFormatter.AppendArgument("url", context.PropertyValue.ToString());

            return context.PropertyValue.ToString().IsValidUrl();
        }
    }
}
