using FluentValidation.Validators;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.RootFolders;

namespace Playarr.Core.Validation.Paths
{
    public class RootFolderExistsValidator : PropertyValidator
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderExistsValidator(IRootFolderService rootFolderService)
        {
            _rootFolderService = rootFolderService;
        }

        protected override string GetDefaultMessageTemplate() => "Root folder '{path}' does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            context.MessageFormatter.AppendArgument("path", context.PropertyValue?.ToString());

            return context.PropertyValue == null || _rootFolderService.All().Exists(r => r.Path.IsPathValid(PathValidationType.CurrentOs) && r.Path.PathEquals(context.PropertyValue.ToString()));
        }
    }
}
