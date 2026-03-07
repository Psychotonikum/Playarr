using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateStandardFilename(SampleResult sampleResult);
        ValidationFailure ValidateDailyFilename(SampleResult sampleResult);
        ValidationFailure ValidateAnimeFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateStandardFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardEpisodeFormat", ERROR_MESSAGE);
            var parsedRomInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedRomInfo == null)
            {
                return validationFailure;
            }

            if (!ValidateSeasonAndRomNumbers(sampleResult.Roms, parsedRomInfo))
            {
                return validationFailure;
            }

            return null;
        }

        public ValidationFailure ValidateDailyFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("DailyEpisodeFormat", ERROR_MESSAGE);
            var parsedRomInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedRomInfo == null)
            {
                return validationFailure;
            }

            if (parsedRomInfo.IsDaily)
            {
                if (!parsedRomInfo.AirDate.Equals(sampleResult.Roms.Single().AirDate))
                {
                    return validationFailure;
                }

                return null;
            }

            if (!ValidateSeasonAndRomNumbers(sampleResult.Roms, parsedRomInfo))
            {
                return validationFailure;
            }

            return null;
        }

        public ValidationFailure ValidateAnimeFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("AnimeEpisodeFormat", ERROR_MESSAGE);
            var parsedRomInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedRomInfo == null)
            {
                return validationFailure;
            }

            if (parsedRomInfo.AbsoluteRomNumbers.Any())
            {
                if (!parsedRomInfo.AbsoluteRomNumbers.First().Equals(sampleResult.Roms.First().AbsoluteEpisodeNumber))
                {
                    return validationFailure;
                }

                return null;
            }

            if (!ValidateSeasonAndRomNumbers(sampleResult.Roms, parsedRomInfo))
            {
                return validationFailure;
            }

            return null;
        }

        private bool ValidateSeasonAndRomNumbers(List<Rom> roms, ParsedRomInfo parsedRomInfo)
        {
            if (parsedRomInfo.SeasonNumber != roms.First().SeasonNumber ||
                !parsedRomInfo.RomNumbers.OrderBy(e => e).SequenceEqual(roms.Select(e => e.EpisodeNumber).OrderBy(e => e)))
            {
                return false;
            }

            return true;
        }
    }
}
