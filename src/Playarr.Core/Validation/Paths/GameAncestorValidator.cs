using System.Linq;
using FluentValidation.Validators;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.Validation.Paths
{
    public class SeriesAncestorValidator : PropertyValidator
    {
        private readonly IGameService _seriesService;

        public SeriesAncestorValidator(IGameService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing game";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return !_seriesService.GetAllSeriesPaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
        }
    }
}
