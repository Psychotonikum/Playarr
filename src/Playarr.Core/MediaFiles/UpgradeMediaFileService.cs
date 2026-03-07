using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        RomFileMoveResult UpgradeRomFile(RomFile romFile, LocalEpisode localRom, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveRomFiles _romFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMoveRomFiles romFileMover,
                                       IDiskProvider diskProvider,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _romFileMover = romFileMover;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public RomFileMoveResult UpgradeRomFile(RomFile romFile, LocalEpisode localRom, bool copyOnly = false)
        {
            var moveFileResult = new RomFileMoveResult();
            var existingFiles = localRom.Roms
                                            .Where(e => e.EpisodeFileId > 0)
                                            .Select(e => e.RomFile.Value)
                                            .Where(e => e != null)
                                            .GroupBy(e => e.Id)
                                            .ToList();

            var rootFolder = _diskProvider.GetParentFolder(localRom.Game.Path);

            // If there are existing rom files and the root folder is missing, throw, so the old file isn't left behind during the import process.
            if (existingFiles.Any() && !_diskProvider.FolderExists(rootFolder))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolder}' was not found.");
            }

            foreach (var existingFile in existingFiles)
            {
                var file = existingFile.First();
                var romFilePath = Path.Combine(localRom.Game.Path, file.RelativePath);
                var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(romFilePath));
                string recycleBinPath = null;

                if (_diskProvider.FileExists(romFilePath))
                {
                    _logger.Debug("Removing existing rom file: {0}", file);
                    recycleBinPath = _recycleBinProvider.DeleteFile(romFilePath, subfolder);
                }
                else
                {
                    _logger.Warn("Existing rom file missing from disk: {0}", romFilePath);
                }

                moveFileResult.OldFiles.Add(new DeletedRomFile(file, recycleBinPath));
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            localRom.OldFiles = moveFileResult.OldFiles;

            if (copyOnly)
            {
                moveFileResult.RomFile = _romFileMover.CopyRomFile(romFile, localRom);
            }
            else
            {
                moveFileResult.RomFile = _romFileMover.MoveRomFile(romFile, localRom);
            }

            return moveFileResult;
        }
    }
}
