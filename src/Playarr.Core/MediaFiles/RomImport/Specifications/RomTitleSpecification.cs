using System;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Download;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class RomTitleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IRomService _romService;
        private readonly Logger _logger;

        public RomTitleSpecification(IConfigService configService,
                                         IBuildFileNames buildFileNames,
                                         IRomService episodeService,
                                         Logger logger)
        {
            _configService = configService;
            _buildFileNames = buildFileNames;
            _romService = episodeService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                _logger.Debug("{0} is in game folder, skipping check", localRom.Path);
                return ImportSpecDecision.Accept();
            }

            var romTitleRequired = _configService.RomTitleRequired;

            if (romTitleRequired == RomTitleRequiredType.Never)
            {
                _logger.Debug("Rom titles are never required, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (!_buildFileNames.RequiresRomTitle(localRom.Game, localRom.Roms))
            {
                _logger.Debug("File name format does not require rom title, skipping check");
                return ImportSpecDecision.Accept();
            }

            var roms = localRom.Roms;
            var firstEpisode = roms.First();
            var episodesInSeason = _romService.GetRomsByPlatform(firstEpisode.GameId, firstEpisode.EpisodeNumber);
            var allEpisodesOnTheSameDay = firstEpisode.AirDateUtc.HasValue && roms.All(e =>
                                              !e.AirDateUtc.HasValue ||
                                              e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value);

            if (romTitleRequired == RomTitleRequiredType.BulkSeasonReleases &&
                allEpisodesOnTheSameDay &&
                episodesInSeason.Count(e => !e.AirDateUtc.HasValue ||
                                            e.AirDateUtc.Value == firstEpisode.AirDateUtc.Value) < 4)
            {
                _logger.Debug("Rom title only required for bulk platform releases");
                return ImportSpecDecision.Accept();
            }

            foreach (var rom in roms)
            {
                var airDateUtc = rom.AirDateUtc;
                var title = rom.Title;

                if (airDateUtc.HasValue && airDateUtc.Value.Before(DateTime.UtcNow.AddHours(-48)))
                {
                    _logger.Debug("Rom aired more than 48 hours ago");
                    continue;
                }

                if (title.IsNullOrWhiteSpace())
                {
                    _logger.Debug("Rom does not have a title and recently aired");

                    return ImportSpecDecision.Reject(ImportRejectionReason.TitleMissing, "Rom does not have a title and recently aired");
                }

                if (title.Equals("TBA"))
                {
                    _logger.Debug("Rom has a TBA title and recently aired");

                    return ImportSpecDecision.Reject(ImportRejectionReason.TitleTba, "Rom has a TBA title and recently aired");
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
