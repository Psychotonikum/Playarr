using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Datastore.Events;
using Playarr.Core.HealthCheck;
using Playarr.Core.Messaging.Events;
using Playarr.SignalR;
using Playarr.Http;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Health
{
    [V3ApiController]
    public class HealthController : RestControllerWithSignalR<HealthResource, HealthCheck>,
                                IHandle<HealthCheckCompleteEvent>
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IBroadcastSignalRMessage signalRBroadcaster, IHealthCheckService healthCheckService)
            : base(signalRBroadcaster)
        {
            _healthCheckService = healthCheckService;
        }

        [NonAction]
        public override ActionResult<HealthResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        protected override HealthResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<HealthResource> GetHealth()
        {
            return _healthCheckService.Results().ToResource();
        }

        [NonAction]
        public void Handle(HealthCheckCompleteEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
