using Playarr.Core.Datastore;
using Playarr.Core.Extras.Files;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Extras.Subtitles
{
    public interface ISubtitleFileRepository : IExtraFileRepository<SubtitleFile>
    {
    }

    public class SubtitleFileRepository : ExtraFileRepository<SubtitleFile>, ISubtitleFileRepository
    {
        public SubtitleFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
