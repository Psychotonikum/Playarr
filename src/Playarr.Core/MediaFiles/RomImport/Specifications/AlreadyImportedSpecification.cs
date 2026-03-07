using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Download;
using Playarr.Core.History;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return ImportSpecDecision.Accept();
            }

            foreach (var rom in localRom.Roms)
            {
                if (!rom.HasFile)
                {
                    _logger.Debug("Skipping already imported check for rom without file");
                    continue;
                }

                var episodeHistory = _historyService.FindByRomId(rom.Id);
                var lastImported = episodeHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId &&
                    h.EventType == EpisodeHistoryEventType.DownloadFolderImported);
                var lastGrabbed = episodeHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId && h.EventType == EpisodeHistoryEventType.Grabbed);

                if (lastImported == null)
                {
                    _logger.Trace("Rom file has not been imported");
                    continue;
                }

                if (lastGrabbed != null)
                {
                    // If the release was grabbed again after importing don't reject it
                    if (lastGrabbed.Date.After(lastImported.Date))
                    {
                        _logger.Trace("Rom file was grabbed again after importing");
                        continue;
                    }

                    // If the release was imported after the last grab reject it
                    if (lastImported.Date.After(lastGrabbed.Date))
                    {
                        _logger.Debug("Rom file previously imported at {0}", lastImported.Date);
                        return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeAlreadyImported, "Rom file already imported at {0}", lastImported.Date.ToLocalTime());
                    }
                }
                else
                {
                    _logger.Debug("Rom file previously imported at {0}", lastImported.Date);
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeAlreadyImported, "Rom file already imported at {0}", lastImported.Date.ToLocalTime());
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
