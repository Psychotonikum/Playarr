using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<RomFile>
    {
        List<RomFile> GetFilesBySeries(int gameId);
        List<RomFile> GetFilesByGameIds(List<int> gameIds);
        List<RomFile> GetFilesBySeason(int gameId, int platformNumber);
        List<RomFile> GetFilesWithoutMediaInfo();
        List<RomFile> GetFilesWithRelativePath(int gameId, string relativePath);
        void DeleteForSeries(List<int> gameIds);
    }

    public class MediaFileRepository : BasicRepository<RomFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<RomFile> GetFilesBySeries(int gameId)
        {
            return Query(c => c.SeriesId == gameId).ToList();
        }

        public List<RomFile> GetFilesByGameIds(List<int> gameIds)
        {
            return Query(c => gameIds.Contains(c.SeriesId)).ToList();
        }

        public List<RomFile> GetFilesBySeason(int gameId, int platformNumber)
        {
            return Query(c => c.SeriesId == gameId && c.SeasonNumber == platformNumber).ToList();
        }

        public List<RomFile> GetFilesWithoutMediaInfo()
        {
            return Query(c => c.MediaInfo == null).ToList();
        }

        public List<RomFile> GetFilesWithRelativePath(int gameId, string relativePath)
        {
            return Query(c => c.SeriesId == gameId && c.RelativePath == relativePath)
                        .ToList();
        }

        public void DeleteForSeries(List<int> gameIds)
        {
            Delete(x => gameIds.Contains(x.SeriesId));
        }
    }
}
