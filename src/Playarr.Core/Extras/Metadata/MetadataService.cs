using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Extras.Files;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.Extras.Others;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata
{
    public class MetadataService : ExtraFileManager<MetadataFile>
    {
        private readonly IMetadataFactory _metadataFactory;
        private readonly ICleanMetadataService _cleanMetadataService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IOtherExtraFileRenamer _otherExtraFileRenamer;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpClient _httpClient;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly Logger _logger;

        public MetadataService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               IRecycleBinProvider recycleBinProvider,
                               IOtherExtraFileRenamer otherExtraFileRenamer,
                               IMetadataFactory metadataFactory,
                               ICleanMetadataService cleanMetadataService,
                               IHttpClient httpClient,
                               IMediaFileAttributeService mediaFileAttributeService,
                               IMetadataFileService metadataFileService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _metadataFactory = metadataFactory;
            _cleanMetadataService = cleanMetadataService;
            _otherExtraFileRenamer = otherExtraFileRenamer;
            _recycleBinProvider = recycleBinProvider;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _httpClient = httpClient;
            _mediaFileAttributeService = mediaFileAttributeService;
            _metadataFileService = metadataFileService;
            _logger = logger;
        }

        public override int Order => 0;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Game game)
        {
            var metadataFiles = _metadataFileService.GetFilesBySeries(game.Id);
            _cleanMetadataService.Clean(game);

            if (!_diskProvider.FolderExists(game.Path))
            {
                _logger.Info("Game folder does not exist, skipping metadata image creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                files.AddRange(ProcessGameImages(consumer, game, consumerFiles));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Game game, List<RomFile> romFiles)
        {
            var metadataFiles = _metadataFileService.GetFilesBySeries(game.Id);
            _cleanMetadataService.Clean(game);

            if (!_diskProvider.FolderExists(game.Path))
            {
                _logger.Info("Game folder does not exist, skipping metadata creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                files.AddIfNotNull(ProcessSeriesMetadata(consumer, game, consumerFiles, SeriesMetadataReason.Scan));
                files.AddRange(ProcessGameImages(consumer, game, consumerFiles));
                files.AddRange(ProcessSeasonImages(consumer, game, consumerFiles));

                foreach (var romFile in romFiles)
                {
                    files.AddIfNotNull(ProcessEpisodeMetadata(consumer, game, romFile, consumerFiles));
                    files.AddRange(ProcessEpisodeImages(consumer, game, romFile, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodesImported(Game game)
        {
            var metadataFiles = _metadataFileService.GetFilesBySeries(game.Id);
            _cleanMetadataService.Clean(game);

            if (!_diskProvider.FolderExists(game.Path))
            {
                _logger.Info("Game folder does not exist, skipping metadata creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                files.AddIfNotNull(ProcessSeriesMetadata(consumer, game, consumerFiles, SeriesMetadataReason.EpisodesImported));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Game game, RomFile romFile)
        {
            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                files.AddIfNotNull(ProcessEpisodeMetadata(consumer, game, romFile, new List<MetadataFile>()));
                files.AddRange(ProcessEpisodeImages(consumer, game, romFile, new List<MetadataFile>()));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Game game, string seriesFolder, string platformFolder)
        {
            var metadataFiles = _metadataFileService.GetFilesBySeries(game.Id);

            if (seriesFolder.IsNullOrWhiteSpace() && platformFolder.IsNullOrWhiteSpace())
            {
                return Array.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                if (seriesFolder.IsNotNullOrWhiteSpace())
                {
                    files.AddIfNotNull(ProcessSeriesMetadata(consumer, game, consumerFiles, SeriesMetadataReason.EpisodeFolderCreated));
                    files.AddRange(ProcessGameImages(consumer, game, consumerFiles));
                }

                if (platformFolder.IsNotNullOrWhiteSpace())
                {
                    files.AddRange(ProcessSeasonImages(consumer, game, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Game game, List<RomFile> romFiles)
        {
            var metadataFiles = _metadataFileService.GetFilesBySeries(game.Id);
            var movedFiles = new List<MetadataFile>();

            // TODO: Move EpisodeImage and EpisodeMetadata metadata files, instead of relying on consumers to do it
            // (Xbmc's EpisodeImage is more than just the extension)

            foreach (var consumer in _metadataFactory.GetAvailableProviders())
            {
                foreach (var romFile in romFiles)
                {
                    var metadataFilesForConsumer = GetMetadataFilesForConsumer(consumer, metadataFiles).Where(m => m.EpisodeFileId == romFile.Id).ToList();

                    foreach (var metadataFile in metadataFilesForConsumer)
                    {
                        var newFileName = consumer.GetFilenameAfterMove(game, romFile, metadataFile);
                        var existingFileName = Path.Combine(game.Path, metadataFile.RelativePath);

                        if (newFileName.PathNotEquals(existingFileName))
                        {
                            try
                            {
                                _diskProvider.MoveFile(existingFileName, newFileName);
                                metadataFile.RelativePath = game.Path.GetRelativePath(newFileName);
                                movedFiles.Add(metadataFile);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to move metadata file after rename: {0}", existingFileName);
                            }
                        }
                    }
                }
            }

            _metadataFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalEpisode localRom, RomFile romFile, string path, string extension, bool readOnly)
        {
            return false;
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalEpisode localRom, RomFile romFile, List<string> files, bool isReadOnly)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        private List<MetadataFile> GetMetadataFilesForConsumer(IMetadata consumer, List<MetadataFile> seriesMetadata)
        {
            return seriesMetadata.Where(c => c.Consumer == consumer.GetType().Name).ToList();
        }

        private MetadataFile ProcessSeriesMetadata(IMetadata consumer, Game game, List<MetadataFile> existingMetadataFiles, SeriesMetadataReason reason)
        {
            var seriesMetadata = consumer.SeriesMetadata(game, reason);

            if (seriesMetadata == null)
            {
                return null;
            }

            var hash = seriesMetadata.Contents.SHA256Hash();

            var metadata = GetMetadataFile(game, existingMetadataFiles, e => e.Type == MetadataType.SeriesMetadata) ??
                               new MetadataFile
                               {
                                   SeriesId = game.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.SeriesMetadata
                               };

            if (hash == metadata.Hash)
            {
                if (seriesMetadata.RelativePath != metadata.RelativePath)
                {
                    metadata.RelativePath = seriesMetadata.RelativePath;

                    return metadata;
                }

                return null;
            }

            var fullPath = Path.Combine(game.Path, seriesMetadata.RelativePath);

            _logger.Debug("Writing Game Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, seriesMetadata.Contents);

            metadata.Hash = hash;
            metadata.RelativePath = seriesMetadata.RelativePath;
            metadata.Extension = Path.GetExtension(fullPath);

            return metadata;
        }

        private MetadataFile ProcessEpisodeMetadata(IMetadata consumer, Game game, RomFile romFile, List<MetadataFile> existingMetadataFiles)
        {
            var episodeMetadata = consumer.EpisodeMetadata(game, romFile);

            if (episodeMetadata == null)
            {
                return null;
            }

            var fullPath = Path.Combine(game.Path, episodeMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(game, fullPath);

            var existingMetadata = GetMetadataFile(game, existingMetadataFiles, c => c.Type == MetadataType.EpisodeMetadata &&
                                                                                  c.EpisodeFileId == romFile.Id);

            if (existingMetadata != null)
            {
                var existingFullPath = Path.Combine(game.Path, existingMetadata.RelativePath);
                if (fullPath.PathNotEquals(existingFullPath))
                {
                    _diskTransferService.TransferFile(existingFullPath, fullPath, TransferMode.Move);
                    existingMetadata.RelativePath = episodeMetadata.RelativePath;
                }
            }

            var hash = episodeMetadata.Contents.SHA256Hash();

            var metadata = existingMetadata ??
                           new MetadataFile
                           {
                               SeriesId = game.Id,
                               SeasonNumber = romFile.SeasonNumber,
                               EpisodeFileId = romFile.Id,
                               Consumer = consumer.GetType().Name,
                               Type = MetadataType.EpisodeMetadata,
                               RelativePath = episodeMetadata.RelativePath,
                               Extension = Path.GetExtension(fullPath)
                           };

            if (hash == metadata.Hash)
            {
                return null;
            }

            _logger.Debug("Writing Rom Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, episodeMetadata.Contents);

            metadata.Hash = hash;

            return metadata;
        }

        private List<MetadataFile> ProcessGameImages(IMetadata consumer, Game game, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var image in consumer.GameImages(game))
            {
                var fullPath = Path.Combine(game.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Game image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(game, fullPath);

                var metadata = GetMetadataFile(game, existingMetadataFiles, c => c.Type == MetadataType.GameImage &&
                                                                              c.RelativePath == image.RelativePath) ??
                               new MetadataFile
                               {
                                   SeriesId = game.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.GameImage,
                                   RelativePath = image.RelativePath,
                                   Extension = Path.GetExtension(fullPath)
                               };

                DownloadImage(game, image);

                result.Add(metadata);
            }

            return result;
        }

        private List<MetadataFile> ProcessSeasonImages(IMetadata consumer, Game game, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var platform in game.Platforms)
            {
                foreach (var image in consumer.SeasonImages(game, platform))
                {
                    var fullPath = Path.Combine(game.Path, image.RelativePath);

                    if (_diskProvider.FileExists(fullPath))
                    {
                        _logger.Debug("Platform image already exists: {0}", fullPath);
                        continue;
                    }

                    _otherExtraFileRenamer.RenameOtherExtraFile(game, fullPath);

                    var metadata = GetMetadataFile(game, existingMetadataFiles, c => c.Type == MetadataType.SeasonImage &&
                                                                                  c.SeasonNumber == platform.SeasonNumber &&
                                                                                  c.RelativePath == image.RelativePath) ??
                                new MetadataFile
                                {
                                    SeriesId = game.Id,
                                    SeasonNumber = platform.SeasonNumber,
                                    Consumer = consumer.GetType().Name,
                                    Type = MetadataType.SeasonImage,
                                    RelativePath = image.RelativePath,
                                    Extension = Path.GetExtension(fullPath)
                                };

                    DownloadImage(game, image);

                    result.Add(metadata);
                }
            }

            return result;
        }

        private List<MetadataFile> ProcessEpisodeImages(IMetadata consumer, Game game, RomFile romFile, List<MetadataFile> existingMetadataFiles)
        {
            var result = new List<MetadataFile>();

            foreach (var image in consumer.EpisodeImages(game, romFile))
            {
                var fullPath = Path.Combine(game.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Rom image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(game, fullPath);

                var existingMetadata = GetMetadataFile(game, existingMetadataFiles, c => c.Type == MetadataType.EpisodeImage &&
                                                                                      c.EpisodeFileId == romFile.Id);

                if (existingMetadata != null)
                {
                    var existingFullPath = Path.Combine(game.Path, existingMetadata.RelativePath);
                    if (fullPath.PathNotEquals(existingFullPath))
                    {
                        _diskTransferService.TransferFile(existingFullPath, fullPath, TransferMode.Move);
                        existingMetadata.RelativePath = image.RelativePath;

                        return new List<MetadataFile> { existingMetadata };
                    }
                }

                var metadata = existingMetadata ??
                               new MetadataFile
                               {
                                   SeriesId = game.Id,
                                   SeasonNumber = romFile.SeasonNumber,
                                   EpisodeFileId = romFile.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.EpisodeImage,
                                   RelativePath = image.RelativePath,
                                   Extension = Path.GetExtension(fullPath)
                               };

                DownloadImage(game, image);

                result.Add(metadata);
            }

            return result;
        }

        private void DownloadImage(Game game, ImageFileResult image)
        {
            var fullPath = Path.Combine(game.Path, image.RelativePath);
            var downloaded = true;

            try
            {
                if (image.Url.StartsWith("http"))
                {
                    _httpClient.DownloadFile(image.Url, fullPath);
                }
                else if (_diskProvider.FileExists(image.Url))
                {
                    _diskProvider.CopyFile(image.Url, fullPath);
                }
                else
                {
                    downloaded = false;
                }

                if (downloaded)
                {
                    _mediaFileAttributeService.SetFilePermissions(fullPath);
                }
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex, "Couldn't download image {0} for {1}. {2}", image.Url, game, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex, "Couldn't download image {0} for {1}. {2}", image.Url, game, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't download image {0} for {1}. {2}", image.Url, game, ex.Message);
            }
        }

        private void SaveMetadataFile(string path, string contents)
        {
            _diskProvider.WriteAllText(path, contents);
            _mediaFileAttributeService.SetFilePermissions(path);
        }

        private MetadataFile GetMetadataFile(Game game, List<MetadataFile> existingMetadataFiles, Func<MetadataFile, bool> predicate)
        {
            var matchingMetadataFiles = existingMetadataFiles.Where(predicate).ToList();

            if (matchingMetadataFiles.Empty())
            {
                return null;
            }

            // Remove duplicate metadata files from DB and disk
            foreach (var file in matchingMetadataFiles.Skip(1))
            {
                var path = Path.Combine(game.Path, file.RelativePath);

                _logger.Debug("Removing duplicate Metadata file: {0}", path);

                var subfolder = _diskProvider.GetParentFolder(game.Path).GetRelativePath(_diskProvider.GetParentFolder(path));
                _recycleBinProvider.DeleteFile(path, subfolder);
                _metadataFileService.Delete(file.Id);
            }

            return matchingMetadataFiles.First();
        }
    }
}
