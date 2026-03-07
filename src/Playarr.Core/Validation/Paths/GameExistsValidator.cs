using System;
using System.Linq;
using FluentValidation.Validators;
using Playarr.Core.Games;

namespace Playarr.Core.Validation.Paths
{
    public class SeriesExistsValidator : PropertyValidator
    {
        private readonly IGameService _seriesService;

        public SeriesExistsValidator(IGameService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "This game has already been added";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var igdbId = Convert.ToInt32(context.PropertyValue.ToString());

            return !_seriesService.AllSeriesIgdbIds().Any(s => s == igdbId);
        }
    }
}
