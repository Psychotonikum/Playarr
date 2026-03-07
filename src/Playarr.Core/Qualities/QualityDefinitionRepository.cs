using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Qualities
{
    public interface IQualityDefinitionRepository : IBasicRepository<QualityDefinition>
    {
    }

    public class QualityDefinitionRepository : BasicRepository<QualityDefinition>, IQualityDefinitionRepository
    {
        public QualityDefinitionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
