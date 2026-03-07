using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.Download;
using Playarr.Core.Extras.Files;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras
{
    public interface IExtraService
    {
        void MoveFilesAfterRename(Game game, RomFile romFile);
        void ImportEpisode(LocalEpisode localRom, RomFile romFile, bool isReadOnly);
    }

    public class ExtraService : IExtraService,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<EpisodeFolderCreatedEvent>,
                                IHandle<SeriesScannedEvent>,
                                IHandle<SeriesRenamedEvent>,
                                IHandle<DownloadsProcessedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRomService _episodeService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly List<IManageExtraFiles> _extraFileManagers;
        private readonly Dictionary<int, Game> _seriesWithImportedFiles;

        public ExtraService(IMediaFileService mediaFileService,
                            IRomService episodeService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            IEnumerable<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _episodeService = episodeService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
            _seriesWithImportedFiles = new Dictionary<int, Game>();
        }

        public void ImportEpisode(LocalEpisode localRom, RomFile romFile, bool isReadOnly)
        {
            ImportExtraFiles(localRom, romFile, isReadOnly);

            CreateAfterEpisodeImport(localRom.Game, romFile);
        }

        private void ImportExtraFiles(LocalEpisode localRom, RomFile romFile, bool isReadOnly)
        {
            if (!_configService.ImportExtraFiles)
            {
                return;
            }

            var folderSearchOption = localRom.FolderRomInfo != null;

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.')
                                                                     .Insert(0, "."))
                                                                     .ToList();

            var sourceFolder = _diskProvider.GetParentFolder(localRom.Path);
            var files = _diskProvider.GetFiles(sourceFolder, folderSearchOption);
            var managedFiles = _extraFileManagers.Select((i) => new List<string>()).ToArray();

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var matchingExtension = wantedExtensions.FirstOrDefault(e => e.Equals(extension));

                if (matchingExtension == null)
                {
                    continue;
                }

                for (var i = 0; i < _extraFileManagers.Count; i++)
                {
                    if (_extraFileManagers[i].CanImportFile(localRom, romFile, file, extension, isReadOnly))
                    {
                        managedFiles[i].Add(file);
                        break;
                    }
                }
            }

            for (var i = 0; i < _extraFileManagers.Count; i++)
            {
                _extraFileManagers[i].ImportFiles(localRom, romFile, managedFiles[i], isReadOnly);
            }
        }

        private void CreateAfterEpisodeImport(Game game, RomFile romFile)
        {
            lock (_seriesWithImportedFiles)
            {
                _seriesWithImportedFiles.TryAdd(game.Id, game);
            }

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeImport(game, romFile);
            }
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                var game = message.Game;

                foreach (var extraFileManager in _extraFileManagers)
                {
                    extraFileManager.CreateAfterMediaCoverUpdate(game);
                }
            }
        }

        public void Handle(SeriesScannedEvent message)
        {
            var game = message.Game;
            var romFiles = GetRomFiles(game.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterSeriesScan(game, romFiles);
            }
        }

        public void Handle(EpisodeFolderCreatedEvent message)
        {
            var game = message.Game;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeFolder(game, message.GameFolder, message.PlatformFolder);
            }
        }

        public void MoveFilesAfterRename(Game game, RomFile romFile)
        {
            var romFiles = new List<RomFile> { romFile };

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(game, romFiles);
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            var game = message.Game;
            var romFiles = GetRomFiles(game.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(game, romFiles);
            }
        }

        public void Handle(DownloadsProcessedEvent message)
        {
            var allGames = new List<Game>();

            lock (_seriesWithImportedFiles)
            {
                allGames.AddRange(_seriesWithImportedFiles.Values);

                _seriesWithImportedFiles.Clear();
            }

            foreach (var game in allGames)
            {
                foreach (var extraFileManager in _extraFileManagers)
                {
                    extraFileManager.CreateAfterEpisodesImported(game);
                }
            }
        }

        private List<RomFile> GetRomFiles(int gameId)
        {
            var romFiles = _mediaFileService.GetFilesBySeries(gameId);
            var roms = _episodeService.GetEpisodeBySeries(gameId);

            foreach (var romFile in romFiles)
            {
                var localRomFile = romFile;
                romFile.Roms = new List<Rom>(roms.Where(e => e.EpisodeFileId == localRomFile.Id));
            }

            return romFiles;
        }
    }
}
