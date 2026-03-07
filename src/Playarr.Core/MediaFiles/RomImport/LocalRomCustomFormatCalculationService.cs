using System.Collections.Generic;
using System.IO;
using Playarr.Core.CustomFormats;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport;

public interface ILocalEpisodeCustomFormatCalculationService
{
    public List<CustomFormat> ParseEpisodeCustomFormats(LocalEpisode localRom);
    public void UpdateEpisodeCustomFormats(LocalEpisode localRom);
}

public class LocalEpisodeCustomFormatCalculationService : ILocalEpisodeCustomFormatCalculationService
{
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public LocalEpisodeCustomFormatCalculationService(IBuildFileNames fileNameBuilder, ICustomFormatCalculationService formatCalculator)
    {
        _fileNameBuilder = fileNameBuilder;
        _formatCalculator = formatCalculator;
    }

    public List<CustomFormat> ParseEpisodeCustomFormats(LocalEpisode localRom)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localRom.Roms, localRom.Game, localRom.ToRomFile());
        return _formatCalculator.ParseCustomFormat(localRom, fileNameUsedForCustomFormatCalculation);
    }

    public void UpdateEpisodeCustomFormats(LocalEpisode localRom)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localRom.Roms, localRom.Game, localRom.ToRomFile());
        localRom.CustomFormats = _formatCalculator.ParseCustomFormat(localRom, fileNameUsedForCustomFormatCalculation);
        localRom.FileNameUsedForCustomFormatCalculation = fileNameUsedForCustomFormatCalculation;
        localRom.CustomFormatScore = localRom.Game.QualityProfile?.Value.CalculateCustomFormatScore(localRom.CustomFormats) ?? 0;

        localRom.OriginalFileNameCustomFormats = _formatCalculator.ParseCustomFormat(localRom, Path.GetFileName(localRom.Path));
        localRom.OriginalFileNameCustomFormatScore = localRom.Game.QualityProfile?.Value.CalculateCustomFormatScore(localRom.OriginalFileNameCustomFormats) ?? 0;
    }
}
