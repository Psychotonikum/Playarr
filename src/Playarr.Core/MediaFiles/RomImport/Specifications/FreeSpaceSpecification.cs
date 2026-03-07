using System;
using System.IO;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FreeSpaceSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (_configService.SkipFreeSpaceCheckWhenImporting)
            {
                _logger.Debug("Skipping free space check when importing");
                return ImportSpecDecision.Accept();
            }

            try
            {
                if (localRom.ExistingFile)
                {
                    _logger.Debug("Skipping free space check for existing rom");
                    return ImportSpecDecision.Accept();
                }

                var path = Directory.GetParent(localRom.Game.Path);
                var freeSpace = _diskProvider.GetAvailableSpace(path.FullName);

                if (!freeSpace.HasValue)
                {
                    _logger.Debug("Free space check returned an invalid result for: {0}", path);
                    return ImportSpecDecision.Accept();
                }

                if (freeSpace < localRom.Size + _configService.MinimumFreeSpaceWhenImporting.Megabytes())
                {
                    _logger.Warn("Not enough free space ({0}) to import: {1} ({2})", freeSpace, localRom, localRom.Size);
                    return ImportSpecDecision.Reject(ImportRejectionReason.MinimumFreeSpace, "Not enough free space");
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error(ex, "Unable to check free disk space while importing.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to check free disk space while importing. {0}", localRom.Path);
            }

            return ImportSpecDecision.Accept();
        }
    }
}
