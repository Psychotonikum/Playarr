using System.Linq;
using FluentValidation.Validators;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.Validation.Paths
{
    public class SeriesPathValidator : PropertyValidator
    {
        private readonly IGameService _seriesService;

        public SeriesPathValidator(IGameService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for another game";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            // Skip the path for this game and any invalid paths
            return !_seriesService.GetAllSeriesPaths().Any(s => s.Key != instanceId &&
                                                                s.Value.IsPathValid(PathValidationType.CurrentOs) &&
                                                                s.Value.PathEquals(context.PropertyValue.ToString()));
        }
    }
}
