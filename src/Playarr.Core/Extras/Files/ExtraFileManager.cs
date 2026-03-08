using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Files
{
    public interface IManageExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Game game);
        IEnumerable<ExtraFile> CreateAfterSeriesScan(Game game, List<RomFile> romFiles);
        IEnumerable<ExtraFile> CreateAfterEpisodesImported(Game game);
        IEnumerable<ExtraFile> CreateAfterEpisodeImport(Game game, RomFile romFile);
        IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Game game, string seriesFolder, string platformFolder);
        IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<RomFile> romFiles);
        bool CanImportFile(LocalEpisode localRom, RomFile romFile, string path, string extension, bool readOnly);
        IEnumerable<ExtraFile> ImportFiles(LocalEpisode localRom, RomFile romFile, List<string> files, bool isReadOnly);
    }

    public abstract class ExtraFileManager<TExtraFile> : IManageExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public ExtraFileManager(IConfigService configService,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService,
                                Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = logger;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Game game);
        public abstract IEnumerable<ExtraFile> CreateAfterSeriesScan(Game game, List<RomFile> romFiles);
        public abstract IEnumerable<ExtraFile> CreateAfterEpisodesImported(Game game);
        public abstract IEnumerable<ExtraFile> CreateAfterEpisodeImport(Game game, RomFile romFile);
        public abstract IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Game game, string seriesFolder, string platformFolder);
        public abstract IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<RomFile> romFiles);
        public abstract bool CanImportFile(LocalEpisode localRom, RomFile romFile, string path, string extension, bool readOnly);
        public abstract IEnumerable<ExtraFile> ImportFiles(LocalEpisode localRom, RomFile romFile, List<string> files, bool isReadOnly);

        protected TExtraFile ImportFile(Game game, RomFile romFile, string path, bool readOnly, string extension, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(Path.Combine(game.Path, romFile.RelativePath));
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(romFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extension);

            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());
            var transferMode = TransferMode.Move;

            if (readOnly)
            {
                transferMode = _configService.CopyUsingHardlinks ? TransferMode.HardLinkOrCopy : TransferMode.Copy;
            }

            _diskTransferService.TransferFile(path, newFileName, transferMode, true);

            return new TExtraFile
            {
                GameId = game.Id,
                PlatformNumber = romFile.PlatformNumber,
                EpisodeFileId = romFile.Id,
                RelativePath = game.Path.GetRelativePath(newFileName),
                Extension = extension
            };
        }

        protected TExtraFile MoveFile(Game game, RomFile romFile, TExtraFile extraFile, string fileNameSuffix = null)
        {
            _logger.Trace("Renaming extra file: {0}", extraFile);

            var newFolder = Path.GetDirectoryName(Path.Combine(game.Path, romFile.RelativePath));
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(romFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extraFile.Extension);

            var existingFileName = Path.Combine(game.Path, extraFile.RelativePath);
            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());

            if (newFileName.PathNotEquals(existingFileName))
            {
                try
                {
                    _logger.Trace("Renaming extra file: {0} to {1}", extraFile, newFileName);

                    _diskProvider.MoveFile(existingFileName, newFileName);
                    extraFile.RelativePath = game.Path.GetRelativePath(newFileName);

                    _logger.Trace("Renamed extra file from: {0}", extraFile);

                    return extraFile;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to move file after rename: {0}", existingFileName);
                }
            }

            return null;
        }
    }
}
