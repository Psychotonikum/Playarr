using System;
using System.Linq;
using FluentValidation.Validators;
using Playarr.Core.Games;

namespace Playarr.Core.Validation.Paths
{
    public class GameExistsValidator : PropertyValidator
    {
        private readonly IGameService _gameService;

        public GameExistsValidator(IGameService seriesService)
        {
            _gameService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "This game has already been added";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var igdbId = Convert.ToInt32(context.PropertyValue.ToString());

            return !_gameService.AllGameIgdbIds().Any(s => s == igdbId);
        }
    }
}
