using FluentValidation.Validators;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;

namespace Playarr.Core.Validation
{
    public class FolderValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid Path: '{path}'";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return context.PropertyValue.ToString().IsPathValid(PathValidationType.CurrentOs);
        }
    }
}
