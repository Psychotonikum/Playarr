using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnsureThat;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles.EpisodeImport;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.RootFolders;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public interface IMoveRomFiles
    {
        RomFile MoveRomFile(RomFile romFile, Game game);
        RomFile MoveRomFile(RomFile romFile, LocalEpisode localRom);
        RomFile CopyRomFile(RomFile romFile, LocalEpisode localRom);
    }

    public class RomFileMovingService : IMoveRomFiles
    {
        private readonly IRomService _episodeService;
        private readonly IUpdateRomFileService _updateRomFileService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IImportScript _scriptImportDecider;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RomFileMovingService(IRomService episodeService,
                                IUpdateRomFileService updateRomFileService,
                                IBuildFileNames buildFileNames,
                                IDiskTransferService diskTransferService,
                                IDiskProvider diskProvider,
                                IMediaFileAttributeService mediaFileAttributeService,
                                IImportScript scriptImportDecider,
                                IRootFolderService rootFolderService,
                                IEventAggregator eventAggregator,
                                IConfigService configService,
                                Logger logger)
        {
            _episodeService = episodeService;
            _updateRomFileService = updateRomFileService;
            _buildFileNames = buildFileNames;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _mediaFileAttributeService = mediaFileAttributeService;
            _scriptImportDecider = scriptImportDecider;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public RomFile MoveRomFile(RomFile romFile, Game game)
        {
            var roms = _episodeService.GetEpisodesByFileId(romFile.Id);
            return MoveRomFile(romFile, game, roms);
        }

        private RomFile MoveRomFile(RomFile romFile, Game game, List<Rom> roms)
        {
            var filePath = _buildFileNames.BuildFilePath(roms, game, romFile, Path.GetExtension(romFile.RelativePath));

            EnsureEpisodeFolder(romFile, game, roms.Select(v => v.PlatformNumber).First(), filePath);

            _logger.Debug("Renaming rom file: {0} to {1}", romFile, filePath);

            return TransferFile(romFile, game, roms, filePath, TransferMode.Move);
        }

        public RomFile MoveRomFile(RomFile romFile, LocalEpisode localRom)
        {
            var filePath = _buildFileNames.BuildFilePath(localRom.Roms, localRom.Game, romFile, Path.GetExtension(localRom.Path), null, localRom.CustomFormats);

            EnsureEpisodeFolder(romFile, localRom, filePath);

            _logger.Debug("Moving rom file: {0} to {1}", romFile.Path, filePath);

            return TransferFile(romFile, localRom.Game, localRom.Roms, filePath, TransferMode.Move, localRom);
        }

        public RomFile CopyRomFile(RomFile romFile, LocalEpisode localRom)
        {
            var filePath = _buildFileNames.BuildFilePath(localRom.Roms, localRom.Game, romFile, Path.GetExtension(localRom.Path), null, localRom.CustomFormats);

            EnsureEpisodeFolder(romFile, localRom, filePath);

            if (_configService.CopyUsingHardlinks)
            {
                _logger.Debug("Attempting to hardlink rom file: {0} to {1}", romFile.Path, filePath);
                return TransferFile(romFile, localRom.Game, localRom.Roms, filePath, TransferMode.HardLinkOrCopy, localRom);
            }

            _logger.Debug("Copying rom file: {0} to {1}", romFile.Path, filePath);
            return TransferFile(romFile, localRom.Game, localRom.Roms, filePath, TransferMode.Copy, localRom);
        }

        private RomFile TransferFile(RomFile romFile, Game game, List<Rom> roms, string destinationFilePath, TransferMode mode, LocalEpisode localRom = null)
        {
            Ensure.That(romFile, () => romFile).IsNotNull();
            Ensure.That(game, () => game).IsNotNull();
            Ensure.That(destinationFilePath, () => destinationFilePath).IsValidPath(PathValidationType.CurrentOs);

            var romFilePath = romFile.Path ?? Path.Combine(game.Path, romFile.RelativePath);

            if (!_diskProvider.FileExists(romFilePath))
            {
                throw new FileNotFoundException("Rom file path does not exist", romFilePath);
            }

            if (romFilePath == destinationFilePath)
            {
                throw new SameFilenameException("File not moved, source and destination are the same", romFilePath);
            }

            romFile.RelativePath = game.Path.GetRelativePath(destinationFilePath);

            if (localRom is not null)
            {
                localRom.FileNameBeforeRename = romFile.RelativePath;
            }

            if (localRom is not null && _scriptImportDecider.TryImport(romFilePath, destinationFilePath, localRom, romFile, mode) is var scriptImportDecision && scriptImportDecision != ScriptImportDecision.DeferMove)
            {
                if (scriptImportDecision == ScriptImportDecision.RenameRequested)
                {
                    try
                    {
                        MoveRomFile(romFile, game, romFile.Roms);
                    }
                    catch (SameFilenameException)
                    {
                        _logger.Debug("No rename was required. File already exists at destination.");
                    }
                }
            }
            else
            {
                _diskTransferService.TransferFile(romFilePath, destinationFilePath, mode);
            }

            _updateRomFileService.ChangeFileDateForFile(romFile, game, roms);

            try
            {
                _mediaFileAttributeService.SetFolderLastWriteTime(game.Path, romFile.DateAdded);

                if (game.PlatformFolder)
                {
                    var platformFolder = Path.GetDirectoryName(destinationFilePath);

                    _mediaFileAttributeService.SetFolderLastWriteTime(platformFolder, romFile.DateAdded);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to set last write time");
            }

            _mediaFileAttributeService.SetFilePermissions(destinationFilePath);

            return romFile;
        }

        private void EnsureEpisodeFolder(RomFile romFile, LocalEpisode localRom, string filePath)
        {
            EnsureEpisodeFolder(romFile, localRom.Game, localRom.PlatformNumber, filePath);
        }

        private void EnsureEpisodeFolder(RomFile romFile, Game game, int platformNumber, string filePath)
        {
            var episodeFolder = Path.GetDirectoryName(filePath);
            var platformFolder = _buildFileNames.BuildSeasonPath(game, platformNumber);
            var seriesFolder = game.Path;
            var rootFolder = _rootFolderService.GetBestRootFolderPath(seriesFolder);

            if (rootFolder.IsNullOrWhiteSpace())
            {
                throw new RootFolderNotFoundException($"Root folder was not found, '{seriesFolder}' is not a subdirectory of a defined root folder.");
            }

            if (!_diskProvider.FolderExists(rootFolder))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolder}' was not found.");
            }

            var changed = false;
            var newEvent = new EpisodeFolderCreatedEvent(game, romFile);

            if (!_diskProvider.FolderExists(seriesFolder))
            {
                CreateFolder(seriesFolder);
                newEvent.GameFolder = seriesFolder;
                changed = true;
            }

            if (seriesFolder != platformFolder && !_diskProvider.FolderExists(platformFolder))
            {
                CreateFolder(platformFolder);
                newEvent.PlatformFolder = platformFolder;
                changed = true;
            }

            if (platformFolder != episodeFolder && !_diskProvider.FolderExists(episodeFolder))
            {
                CreateFolder(episodeFolder);
                newEvent.EpisodeFolder = episodeFolder;
                changed = true;
            }

            if (changed)
            {
                _eventAggregator.PublishEvent(newEvent);
            }
        }

        private void CreateFolder(string directoryName)
        {
            Ensure.That(directoryName, () => directoryName).IsNotNullOrWhiteSpace();

            var parentFolder = new OsPath(directoryName).Directory.FullPath;
            if (!_diskProvider.FolderExists(parentFolder))
            {
                CreateFolder(parentFolder);
            }

            try
            {
                _diskProvider.CreateFolder(directoryName);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to create directory: {0}", directoryName);
            }

            _mediaFileAttributeService.SetFolderPermissions(directoryName);
        }
    }
}
