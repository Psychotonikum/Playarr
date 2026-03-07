using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore.Events;
using Playarr.Core.Download.Pending;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Queue;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Queue
{
    [V5ApiController("queue/details")]
    public class QueueDetailsController : RestControllerWithSignalR<QueueResource, Playarr.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsController(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
        }

        [NonAction]
        public override ActionResult<QueueResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        protected override QueueResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<QueueResource> GetQueue(int? gameId, [FromQuery]List<int> romIds, [FromQuery] QueueSubresource[]? includeSubresources = null)
        {
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);
            var includeSeries = includeSubresources.Contains(QueueSubresource.Game);
            var includeEpisodes = includeSubresources.Contains(QueueSubresource.Roms);

            if (gameId.HasValue)
            {
                return fullQueue.Where(q => q.Game?.Id == gameId).ToResource(includeSeries, includeEpisodes);
            }

            if (romIds.Any())
            {
                return fullQueue.Where(q => q.Roms.Any() &&
                                            romIds.IntersectBy(e => e, q.Roms, e => e.Id, null).Any())
                    .ToResource(includeSeries, includeEpisodes);
            }

            return fullQueue.ToResource(includeSeries, includeEpisodes);
        }

        [NonAction]
        public void Handle(QueueUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        [NonAction]
        public void Handle(PendingReleasesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
