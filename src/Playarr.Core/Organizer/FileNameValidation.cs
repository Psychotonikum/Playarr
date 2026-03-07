using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using Playarr.Common.Extensions;
using Playarr.Core.MediaFiles;

namespace Playarr.Core.Organizer
{
    public static class FileNameValidation
    {
        private static readonly Regex PlatformFolderRegex = new Regex(@"(\{platform(\:\d+)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static readonly Regex OriginalTokenRegex = new Regex(@"(\{original[- ._](?:title|filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidStandardEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidDailyEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidDailyEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidAnimeEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidAnimeEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidGameFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.GameTitleRegex)).WithMessage("Must contain game title");
        }

        public static IRuleBuilderOptions<T, string> ValidPlatformFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(PlatformFolderRegex)).WithMessage("Must contain platform number");
        }

        public static IRuleBuilderOptions<T, string> ValidSpecialsFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new FilteredSubfolderValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidCustomColonReplacement<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new IllegalColonCharactersValidator());

            return ruleBuilder.SetValidator(new IllegalCharactersValidator());
        }
    }

    public class ValidStandardEpisodeFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain platform and rom numbers OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) ||
                   (FileNameBuilder.SeasonRegex.IsMatch(value) && FileNameBuilder.EpisodeRegex.IsMatch(value)) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
        }
    }

    public class ValidDailyEpisodeFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain Air Date OR Platform and Rom OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) ||
                   (FileNameBuilder.SeasonRegex.IsMatch(value) && FileNameBuilder.EpisodeRegex.IsMatch(value)) ||
                   FileNameBuilder.AirDateRegex.IsMatch(value) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
        }
    }

    public class ValidAnimeEpisodeFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() =>
            "Must contain Absolute Rom number OR Platform and Rom OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) ||
                   (FileNameBuilder.SeasonRegex.IsMatch(value) && FileNameBuilder.EpisodeRegex.IsMatch(value)) ||
                   FileNameBuilder.AbsoluteEpisodePatternRegex.IsMatch(value) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
        }
    }

    public class IllegalCharactersValidator : PropertyValidator
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        protected override string GetDefaultMessageTemplate() => "Contains illegal characters: {InvalidCharacters}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;
            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var invalidCharacters = InvalidPathChars.Where(i => value!.IndexOf(i) >= 0).ToList();

            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }

    public class IllegalColonCharactersValidator : PropertyValidator
    {
        private static readonly string[] InvalidPathChars = FileNameBuilder.BadCharacters.Concat(new[] { ":" }).ToArray();

        protected override string GetDefaultMessageTemplate() => "Contains illegal characters: {InvalidCharacters}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var invalidCharacters = InvalidPathChars.Where(i => value!.IndexOf(i, StringComparison.Ordinal) >= 0).ToList();

            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }

    public class FilteredSubfolderValidator : PropertyValidator
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        protected override string GetDefaultMessageTemplate() => "Matches excluded subfolder pattern: {FilteredSubfolders}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var subfolder = value + Path.DirectorySeparatorChar;
            var matches = DiskScanService.FilteredSubFolderMatches(subfolder);

            if (matches.Any())
            {
                context.MessageFormatter.AppendArgument("FilteredSubfolders", string.Join("", matches.Select(m => m.TrimEnd(Path.DirectorySeparatorChar))));
                return false;
            }

            return true;
        }
    }
}
