using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Commands;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Games
{
    public class SeriesAddedHandler : IHandle<SeriesAddedEvent>,
                                      IHandle<GameImportedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public SeriesAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(SeriesAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshSeriesCommand(new List<int> { message.Game.Id }, true));
        }

        public void Handle(GameImportedEvent message)
        {
            _commandQueueManager.PushMany(message.GameIds.Select(s => new RefreshSeriesCommand(new List<int> { s }, true)).ToList());
        }
    }
}
