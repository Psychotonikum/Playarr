using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Datastore.Events;
using Playarr.Core.Download.Pending;
using Playarr.Core.Download.TrackedDownloads;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Queue;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;
using Debouncer = Playarr.Common.TPL.Debouncer;

namespace Playarr.Api.V5.Queue
{
    [V5ApiController("queue/status")]
    public class QueueStatusController : RestControllerWithSignalR<QueueStatusResource, Playarr.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Debouncer _broadcastDebounce;

        public QueueStatusController(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;

            _broadcastDebounce = new Debouncer(BroadcastChange, TimeSpan.FromSeconds(5));
        }

        [NonAction]
        public override ActionResult<QueueStatusResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        [HttpGet]
        [Produces("application/json")]
        public QueueStatusResource GetQueueStatus()
        {
            _broadcastDebounce.Pause();

            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();

            var resource = new QueueStatusResource
            {
                TotalCount = queue.Count + pending.Count,
                Count = queue.Count(q => q.Game != null) + pending.Count,
                UnknownCount = queue.Count(q => q.Game == null),
                Errors = queue.Any(q => q.Game != null && q.TrackedDownloadStatus == TrackedDownloadStatus.Error),
                Warnings = queue.Any(q => q.Game != null && q.TrackedDownloadStatus == TrackedDownloadStatus.Warning),
                UnknownErrors = queue.Any(q => q.Game == null && q.TrackedDownloadStatus == TrackedDownloadStatus.Error),
                UnknownWarnings = queue.Any(q => q.Game == null && q.TrackedDownloadStatus == TrackedDownloadStatus.Warning)
            };

            _broadcastDebounce.Resume();

            return resource;
        }

        private void BroadcastChange()
        {
            BroadcastResourceChange(ModelAction.Updated, GetQueueStatus());
        }

        [NonAction]
        public void Handle(QueueUpdatedEvent message)
        {
            _broadcastDebounce.Execute();
        }

        [NonAction]
        public void Handle(PendingReleasesUpdatedEvent message)
        {
            _broadcastDebounce.Execute();
        }
    }
}
