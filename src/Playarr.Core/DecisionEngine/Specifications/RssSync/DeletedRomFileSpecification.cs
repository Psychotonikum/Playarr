using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.DecisionEngine.Specifications.RssSync
{
    public class DeletedRomFileSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DeletedRomFileSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Disk;
        public RejectionType Type => RejectionType.Temporary;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            if (!_configService.AutoUnmonitorPreviouslyDownloadedEpisodes)
            {
                return DownloadSpecDecision.Accept();
            }

            if (information.SearchCriteria != null)
            {
                _logger.Debug("Skipping deleted episodefile check during search");
                return DownloadSpecDecision.Accept();
            }

            var missingRomFiles = subject.Roms
                                             .Where(v => v.EpisodeFileId != 0)
                                             .Select(v => v.RomFile.Value)
                                             .DistinctBy(v => v.Id)
                                             .Where(v => IsRomFileMissing(subject.Game, v))
                                             .ToArray();

            if (missingRomFiles.Any())
            {
                foreach (var missingRomFile in missingRomFiles)
                {
                    _logger.Trace("Rom file {0} is missing from disk.", missingRomFile.RelativePath);
                }

                _logger.Debug("Files for this rom exist in the database but not on disk, will be unmonitored on next diskscan. skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotMonitored, "Rom is not monitored");
            }

            return DownloadSpecDecision.Accept();
        }

        private bool IsRomFileMissing(Game game, RomFile romFile)
        {
            var fullPath = Path.Combine(game.Path, romFile.RelativePath);

            return !_diskProvider.FileExists(fullPath);
        }
    }
}
