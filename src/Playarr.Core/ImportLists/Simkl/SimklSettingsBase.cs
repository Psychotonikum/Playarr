using System;
using FluentValidation;
using Playarr.Common.Extensions;
using Playarr.Core.Annotations;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Simkl
{
    public class SimklSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : SimklSettingsBase<TSettings>
    {
        public SimklSettingsBaseValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with Simkl");

            RuleFor(c => c.Expires).NotEmpty()
                                   .OverridePropertyName("SignIn")
                                   .WithMessage("Must authenticate with Simkl")
                                   .When(c => c.AccessToken.IsNotNullOrWhiteSpace() && c.RefreshToken.IsNotNullOrWhiteSpace());
        }
    }

    public class SimklSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : SimklSettingsBase<TSettings>
    {
        private static readonly SimklSettingsBaseValidator<TSettings> Validator = new();

        public SimklSettingsBase()
        {
            SignIn = "startOAuth";
        }

        public override string BaseUrl { get; set; } = "https://api.simkl.com";

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsAuthUser", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(99, Label = "ImportListsSimklSettingsAuthenticatewithSimkl", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override PlayarrValidationResult Validate()
        {
            return new PlayarrValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
