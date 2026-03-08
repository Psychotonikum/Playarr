using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Games
{
    public interface IGameSystemRepository : IBasicRepository<GameSystem>
    {
        GameSystem FindByName(string name);
        GameSystem FindByFolderName(string folderName);
        List<GameSystem> GetBySystemType(GameSystemType systemType);
    }

    public class GameSystemRepository : BasicRepository<GameSystem>, IGameSystemRepository
    {
        public GameSystemRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public GameSystem FindByName(string name)
        {
            return Query(s => s.Name == name).SingleOrDefault();
        }

        public GameSystem FindByFolderName(string folderName)
        {
            return Query(s => s.FolderName == folderName).SingleOrDefault();
        }

        public List<GameSystem> GetBySystemType(GameSystemType systemType)
        {
            return Query(s => s.SystemType == systemType);
        }
    }
}
