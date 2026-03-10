using FluentValidation;
using FluentValidation.Results;
using Playarr.Core.Validation.Paths;

namespace Playarr.Core.Games
{
    public interface IAddGameValidator
    {
        ValidationResult Validate(Game instance);
    }

    public class AddGameValidator : AbstractValidator<Game>, IAddGameValidator
    {
        public AddGameValidator(RootFolderValidator rootFolderValidator,
                                  GamePathValidator seriesPathValidator,
                                  GameAncestorValidator seriesAncestorValidator,
                                  GameTitleSlugValidator gameTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(seriesPathValidator)
                                .SetValidator(seriesAncestorValidator);

            RuleFor(c => c.TitleSlug).SetValidator(gameTitleSlugValidator);
        }
    }
}
