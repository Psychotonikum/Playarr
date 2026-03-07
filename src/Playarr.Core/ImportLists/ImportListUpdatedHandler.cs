using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.ThingiProvider.Events;

namespace Playarr.Core.ImportLists
{
    public class ImportListUpdatedHandler : IHandle<ProviderUpdatedEvent<IImportList>>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public ImportListUpdatedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ProviderUpdatedEvent<IImportList> message)
        {
            _commandQueueManager.Push(new ImportListSyncCommand(message.Definition.Id));
        }
    }
}
