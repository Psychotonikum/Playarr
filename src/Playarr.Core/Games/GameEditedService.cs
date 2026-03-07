using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Commands;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public class SeriesEditedService : IHandle<SeriesEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public SeriesEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(SeriesEditedEvent message)
        {
            if (message.Game.SeriesType != message.OldSeries.SeriesType)
            {
                _commandQueueManager.Push(new RefreshSeriesCommand(new List<int> { message.Game.Id }, false));
            }
        }
    }
}
