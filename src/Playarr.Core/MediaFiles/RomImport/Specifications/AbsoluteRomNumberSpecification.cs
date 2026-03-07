using System;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AbsoluteRomNumberSpecification : IImportDecisionEngineSpecification
    {
        private readonly IBuildFileNames _buildFileNames;
        private readonly Logger _logger;

        public AbsoluteRomNumberSpecification(IBuildFileNames buildFileNames, Logger logger)
        {
            _buildFileNames = buildFileNames;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.Game.SeriesType != GameTypes.Anime)
            {
                _logger.Debug("Game type is not Anime, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (!_buildFileNames.RequiresAbsoluteRomNumber())
            {
                _logger.Debug("File name format does not require absolute rom number, skipping check");
                return ImportSpecDecision.Accept();
            }

            foreach (var rom in localRom.Roms)
            {
                var airDateUtc = rom.AirDateUtc;
                var absoluteRomNumber = rom.AbsoluteEpisodeNumber;

                if (airDateUtc.HasValue && airDateUtc.Value.Before(DateTime.UtcNow.AddDays(-1)))
                {
                    _logger.Debug("Rom aired more than 1 day ago");
                    continue;
                }

                if (!absoluteRomNumber.HasValue)
                {
                    _logger.Debug("Rom does not have an absolute rom number and recently aired");

                    return ImportSpecDecision.Reject(ImportRejectionReason.MissingAbsoluteRomNumber, "Rom does not have an absolute rom number and recently aired");
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
