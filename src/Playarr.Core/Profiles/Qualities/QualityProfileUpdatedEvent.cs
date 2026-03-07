using Playarr.Common.Messaging;

namespace Playarr.Core.Profiles.Qualities;

public class QualityProfileUpdatedEvent(int id) : IEvent
{
    public int Id { get; private set; } = id;
}
