using System.Collections.Generic;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.DataAugmentation.Scene
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        List<SceneMapping> FindByTvdbid(int igdbId);
        void Clear(string type);
    }

    public class SceneMappingRepository : BasicRepository<SceneMapping>, ISceneMappingRepository
    {
        public SceneMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SceneMapping> FindByTvdbid(int igdbId)
        {
            return Query(x => x.TvdbId == igdbId);
        }

        public void Clear(string type)
        {
            Delete(s => s.Type == type);
        }
    }
}
