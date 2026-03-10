using System.Linq;
using FluentValidation.Validators;
using Playarr.Common.Extensions;

namespace Playarr.Core.Games
{
    public class GameTitleSlugValidator : PropertyValidator
    {
        private readonly IGameService _gameService;

        public GameTitleSlugValidator(IGameService seriesService)
        {
            _gameService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() =>
            "Title slug '{slug}' is in use by game '{gameTitle}'. Check the FAQ for more information";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;
            var slug = context.PropertyValue.ToString();

            var conflictingSeries = _gameService.GetAllGames()
                                                  .FirstOrDefault(s => s.TitleSlug.IsNotNullOrWhiteSpace() &&
                                                              s.TitleSlug.Equals(context.PropertyValue.ToString()) &&
                                                              s.Id != instanceId);

            if (conflictingSeries == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("slug", slug);
            context.MessageFormatter.AppendArgument("gameTitle", conflictingSeries.Title);

            return false;
        }
    }
}
