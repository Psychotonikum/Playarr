using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.MediaFiles
{
    public interface IMediaFileService
    {
        RomFile Add(RomFile romFile);
        void Update(RomFile romFile);
        void Update(List<RomFile> romFiles);
        void Delete(RomFile romFile, DeleteMediaFileReason reason);
        List<RomFile> GetFilesBySeries(int gameId);
        List<RomFile> GetFilesByGameIds(List<int> gameIds);
        List<RomFile> GetFilesBySeason(int gameId, int platformNumber);
        List<RomFile> GetFiles(IEnumerable<int> ids);
        List<RomFile> GetFilesWithoutMediaInfo();
        List<string> FilterExistingFiles(List<string> files, Game game);
        RomFile Get(int id);
        List<RomFile> Get(IEnumerable<int> ids);
        List<RomFile> GetFilesWithRelativePath(int gameId, string relativePath);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IEventAggregator eventAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public RomFile Add(RomFile romFile)
        {
            var addedFile = _mediaFileRepository.Insert(romFile);
            _eventAggregator.PublishEvent(new RomFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(RomFile romFile)
        {
            _mediaFileRepository.Update(romFile);
        }

        public void Update(List<RomFile> romFiles)
        {
            _mediaFileRepository.UpdateMany(romFiles);
        }

        public void Delete(RomFile romFile, DeleteMediaFileReason reason)
        {
            // Little hack so we have the roms and game attached for the event consumers
            romFile.Roms.LazyLoad();
            romFile.Path = Path.Combine(romFile.Game.Value.Path, romFile.RelativePath);

            _mediaFileRepository.Delete(romFile);
            _eventAggregator.PublishEvent(new RomFileDeletedEvent(romFile, reason));
        }

        public List<RomFile> GetFilesBySeries(int gameId)
        {
            return _mediaFileRepository.GetFilesBySeries(gameId);
        }

        public List<RomFile> GetFilesByGameIds(List<int> gameIds)
        {
            return _mediaFileRepository.GetFilesByGameIds(gameIds);
        }

        public List<RomFile> GetFilesBySeason(int gameId, int platformNumber)
        {
            return _mediaFileRepository.GetFilesBySeason(gameId, platformNumber);
        }

        public List<RomFile> GetFiles(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public List<RomFile> GetFilesWithoutMediaInfo()
        {
            return _mediaFileRepository.GetFilesWithoutMediaInfo();
        }

        public List<string> FilterExistingFiles(List<string> files, Game game)
        {
            var seriesFiles = GetFilesBySeries(game.Id);

            return FilterExistingFiles(files, seriesFiles, game);
        }

        public RomFile Get(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<RomFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public List<RomFile> GetFilesWithRelativePath(int gameId, string relativePath)
        {
            return _mediaFileRepository.GetFilesWithRelativePath(gameId, relativePath);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            _mediaFileRepository.DeleteForSeries(message.Game.Select(s => s.Id).ToList());
        }

        public static List<string> FilterExistingFiles(List<string> files, List<RomFile> seriesFiles, Game game)
        {
            var seriesFilePaths = seriesFiles.Select(f => Path.Combine(game.Path, f.RelativePath)).ToList();

            if (!seriesFilePaths.Any())
            {
                return files;
            }

            return files.Except(seriesFilePaths, PathEqualityComparer.Instance).ToList();
        }
    }
}
