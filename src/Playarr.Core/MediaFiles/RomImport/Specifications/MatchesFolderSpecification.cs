using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var fileInfo = localRom.FileRomInfo;
            var folderInfo = localRom.FolderRomInfo;

            if (fileInfo != null && fileInfo.IsPossibleSceneSeasonSpecial)
            {
                fileInfo = _parsingService.ParseSpecialRomTitle(fileInfo, fileInfo.ReleaseTitle, localRom.Game.IgdbId, 0, null);
            }

            if (folderInfo != null && folderInfo.IsPossibleSceneSeasonSpecial)
            {
                folderInfo = _parsingService.ParseSpecialRomTitle(folderInfo, folderInfo.ReleaseTitle, localRom.Game.IgdbId, 0, null);
            }

            if (folderInfo == null)
            {
                _logger.Debug("No folder ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (fileInfo == null)
            {
                _logger.Debug("No file ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var folderEpisodes = _parsingService.GetEpisodes(folderInfo, localRom.Game, true);
            var fileEpisodes = _parsingService.GetEpisodes(fileInfo, localRom.Game, true);

            if (folderEpisodes.Empty())
            {
                _logger.Debug("No rom numbers in folder ParsedRomInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var unexpected = fileEpisodes.Where(e => folderEpisodes.All(o => o.Id != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected rom(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeUnexpected, "Rom {0} was unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeUnexpected, "Roms {0} were unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
            }

            return ImportSpecDecision.Accept();
        }

        private string FormatEpisode(List<Rom> roms)
        {
            return string.Join(", ", roms.Select(e => $"{e.PlatformNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
