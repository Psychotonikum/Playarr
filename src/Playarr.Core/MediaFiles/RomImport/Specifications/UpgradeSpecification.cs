using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFormats;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    ICustomFormatCalculationService formatService,
                                    Logger logger)
        {
            _configService = configService;
            _formatService = formatService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;
            var qualityProfile = localRom.Game.QualityProfile.Value;
            var qualityComparer = new QualityModelComparer(qualityProfile);

            foreach (var rom in localRom.Roms.Where(e => e.EpisodeFileId > 0))
            {
                var romFile = rom.RomFile.Value;

                if (romFile == null)
                {
                    _logger.Trace("Unable to get rom file details from the DB. EpisodeId: {0} EpisodeFileId: {1}", rom.Id, rom.EpisodeFileId);
                    continue;
                }

                var qualityCompare = qualityComparer.Compare(localRom.Quality.Quality, romFile.Quality.Quality);

                if (qualityCompare < 0)
                {
                    _logger.Debug("This file isn't a quality upgrade for all roms. Existing quality: {0}. New Quality {1}. Skipping {2}", romFile.Quality.Quality, localRom.Quality.Quality, localRom.Path);
                    return ImportSpecDecision.Reject(ImportRejectionReason.NotQualityUpgrade, "Not an upgrade for existing rom file(s). Existing quality: {0}. New Quality {1}.", romFile.Quality.Quality, localRom.Quality.Quality);
                }

                // Same quality, propers/repacks are preferred and it is not a revision update. Reject revision downgrade.

                if (qualityCompare == 0 &&
                    downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                    localRom.Quality.Revision.CompareTo(romFile.Quality.Revision) < 0)
                {
                    _logger.Debug("This file isn't a quality revision upgrade for all roms. Skipping {0}", localRom.Path);
                    return ImportSpecDecision.Reject(ImportRejectionReason.NotRevisionUpgrade, "Not a quality revision upgrade for existing rom file(s)");
                }

                var currentFormats = _formatService.ParseCustomFormat(romFile);
                var currentFormatScore = qualityProfile.CalculateCustomFormatScore(currentFormats);
                var newFormats = localRom.CustomFormats;
                var newFormatScore = localRom.CustomFormatScore;
                var newFormatsBeforeRename = localRom.OriginalFileNameCustomFormats;
                var newFormatScoreBeforeRename = localRom.OriginalFileNameCustomFormatScore;

                if (qualityCompare == 0 && newFormatScore < currentFormatScore)
                {
                    _logger.Debug("New item's custom formats [{0}] ({1}) do not improve on [{2}] ({3}), skipping",
                        newFormats != null ? newFormats.ConcatToString() : "",
                        newFormatScore,
                        currentFormats != null ? currentFormats.ConcatToString() : "",
                        currentFormatScore);

                    if (newFormatScoreBeforeRename > currentFormatScore)
                    {
                        return ImportSpecDecision.Reject(ImportRejectionReason.NotCustomFormatUpgradeAfterRename,
                            "Not a Custom Format upgrade for existing rom file(s). AfterRename: [{0}] ({1}) do not improve on Existing: [{2}] ({3}) even though BeforeRename: [{4}] ({5}) did.",
                            newFormats != null ? newFormats.ConcatToString() : "",
                            newFormatScore,
                            currentFormats != null ? currentFormats.ConcatToString() : "",
                            currentFormatScore,
                            newFormatsBeforeRename != null ? newFormatsBeforeRename.ConcatToString() : "",
                            newFormatScoreBeforeRename);
                    }

                    return ImportSpecDecision.Reject(ImportRejectionReason.NotCustomFormatUpgrade,
                        "Not a Custom Format upgrade for existing rom file(s). New: [{0}] ({1}) do not improve on Existing: [{2}] ({3})",
                        newFormats != null ? newFormats.ConcatToString() : "",
                        newFormatScore,
                        currentFormats != null ? currentFormats.ConcatToString() : "",
                        currentFormatScore);
                }

                _logger.Debug("New item's custom formats [{0}] ({1}) improve on [{2}] ({3}), accepting",
                    newFormats != null ? newFormats.ConcatToString() : "",
                    newFormatScore,
                    currentFormats != null ? currentFormats.ConcatToString() : "",
                    currentFormatScore);
            }

            return ImportSpecDecision.Accept();
        }
    }
}
