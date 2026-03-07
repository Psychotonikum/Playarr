using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Validation;
using Playarr.Core.Validation.Paths;

namespace Playarr.Api.V3.Game
{
    public class GameEditorValidator : AbstractValidator<Playarr.Core.Games.Game>
    {
        public GameEditorValidator(RootFolderExistsValidator rootFolderExistsValidator, QualityProfileExistsValidator qualityProfileExistsValidator)
        {
            RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => s.RootFolderPath.IsNotNullOrWhiteSpace());

            RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }
    }
}
