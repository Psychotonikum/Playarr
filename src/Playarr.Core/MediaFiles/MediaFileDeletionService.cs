using System;
using System.IO;
using System.Net;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Exceptions;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteRomFile(Game game, RomFile romFile);
    }

    public class MediaFileDeletionService : IDeleteMediaFiles,
                                            IHandleAsync<SeriesDeletedEvent>,
                                            IHandle<RomFileDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IGameService _gameService;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        IGameService seriesService,
                                        IConfigService configService,
                                        IEventAggregator eventAggregator,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _gameService = seriesService;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DeleteRomFile(Game game, RomFile romFile)
        {
            var fullPath = Path.Combine(game.Path, romFile.RelativePath);
            var rootFolder = _diskProvider.GetParentFolder(game.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Game' root folder ({0}) doesn't exist.", rootFolder);
                throw new PlayarrClientException(HttpStatusCode.Conflict, "Game' root folder ({0}) doesn't exist.", rootFolder);
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Game' root folder ({0}) is empty.", rootFolder);
                throw new PlayarrClientException(HttpStatusCode.Conflict, "Game' root folder ({0}) is empty.", rootFolder);
            }

            if (_diskProvider.FolderExists(game.Path) && _diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting rom file: {0}", fullPath);

                var subfolder = _diskProvider.GetParentFolder(game.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));

                try
                {
                    _recycleBinProvider.DeleteFile(fullPath, subfolder);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete rom file");
                    throw new PlayarrClientException(HttpStatusCode.InternalServerError, "Unable to delete rom file");
                }
            }

            // Delete the rom file from the database to clean it up even if the file was already deleted
            _mediaFileService.Delete(romFile, DeleteMediaFileReason.Manual);

            _eventAggregator.PublishEvent(new DeleteCompletedEvent());
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                var allGames = _gameService.GetAllGamePaths();

                foreach (var game in message.Game)
                {
                    foreach (var s in allGames)
                    {
                        if (s.Key == game.Id)
                        {
                            continue;
                        }

                        if (game.Path.IsParentPath(s.Value))
                        {
                            _logger.Error("Game path: '{0}' is a parent of another game, not deleting files.", game.Path);
                            return;
                        }

                        if (game.Path.PathEquals(s.Value))
                        {
                            _logger.Error("Game path: '{0}' is the same as another game, not deleting files.", game.Path);
                            return;
                        }
                    }

                    if (_diskProvider.FolderExists(game.Path))
                    {
                        _recycleBinProvider.DeleteFolder(game.Path);
                    }

                    _eventAggregator.PublishEvent(new DeleteCompletedEvent());
                }
            }
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(RomFileDeletedEvent message)
        {
            if (!_configService.DeleteEmptyFolders || message.Reason == DeleteMediaFileReason.MissingFromDisk)
            {
                return;
            }

            var game = message.RomFile.Game.Value;
            var gamePath = game.Path;
            var folder = message.RomFile.Path.GetParentPath();

            while (gamePath.IsParentPath(folder))
            {
                if (_diskProvider.FolderExists(folder))
                {
                    _diskProvider.RemoveEmptySubfolders(folder);
                }

                folder = folder.GetParentPath();
            }

            _diskProvider.RemoveEmptySubfolders(gamePath);

            if (_diskProvider.FolderEmpty(gamePath))
            {
                _diskProvider.DeleteFolder(gamePath, true);
            }
        }
    }
}
