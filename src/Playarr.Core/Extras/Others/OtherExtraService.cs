using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Extras.Files;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IOtherExtraFileService _otherExtraFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IMediaFileAttributeService mediaFileAttributeService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _diskProvider = diskProvider;
            _otherExtraFileService = otherExtraFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Game game)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Game game, List<RomFile> romFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodesImported(Game game)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Game game, RomFile romFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Game game, string seriesFolder, string platformFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<RomFile> romFiles)
        {
            var extraFiles = _otherExtraFileService.GetFilesBySeries(game.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var romFile in romFiles)
            {
                var extraFilesForRomFile = extraFiles.Where(m => m.EpisodeFileId == romFile.Id).ToList();

                foreach (var extraFile in extraFilesForRomFile)
                {
                    movedFiles.AddIfNotNull(MoveFile(game, romFile, extraFile));
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalEpisode localRom, RomFile romFile, string path, string extension, bool readOnly)
        {
            return true;
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalEpisode localRom, RomFile romFile, List<string> files, bool isReadOnly)
        {
            var importedFiles = new List<ExtraFile>();
            var filteredFiles = files.Where(f => CanImportFile(localRom, romFile, f, Path.GetExtension(f), isReadOnly)).ToList();
            var sourcePath = localRom.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var matchingFiles = new List<string>();
            var hasNfo = false;

            foreach (var file in filteredFiles)
            {
                try
                {
                    // Filter out duplicate NFO files
                    if (file.EndsWith(".nfo", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (hasNfo)
                        {
                            continue;
                        }

                        hasNfo = true;
                    }

                    // Filename match
                    if (Path.GetFileNameWithoutExtension(file).StartsWith(sourceFileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingFiles.Add(file);
                        continue;
                    }

                    // Platform and rom match
                    var fileRomInfo = Parser.Parser.ParsePath(file) ?? new ParsedRomInfo();

                    if (fileRomInfo.RomNumbers.Length == 0)
                    {
                        continue;
                    }

                    if (fileRomInfo.PlatformNumber == localRom.FileRomInfo.PlatformNumber &&
                        fileRomInfo.RomNumbers.SequenceEqual(localRom.FileRomInfo.RomNumbers))
                    {
                        matchingFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            foreach (var file in matchingFiles)
            {
                try
                {
                    var extraFile = ImportFile(localRom.Game, romFile, file, isReadOnly, Path.GetExtension(file), null);
                    _mediaFileAttributeService.SetFilePermissions(file);
                    _otherExtraFileService.Upsert(extraFile);
                    importedFiles.Add(extraFile);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import extra file: {0}", file);
                }
            }

            return importedFiles;
        }
    }
}
