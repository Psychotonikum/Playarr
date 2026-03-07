using NLog;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Lifecycle.Commands;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Lifecycle
{
    public interface ILifecycleService
    {
        void Shutdown();
        void Restart();
    }

    public class LifecycleService : ILifecycleService, IExecute<ShutdownCommand>, IExecute<RestartCommand>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public LifecycleService(IEventAggregator eventAggregator,
                                IRuntimeInfo runtimeInfo,
                                Logger logger)
        {
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _logger = logger;
        }

        public void Shutdown()
        {
            _logger.Info("Shutdown requested.");
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested(false));
        }

        public void Restart()
        {
            _logger.Info("Restart requested.");
            _runtimeInfo.RestartPending = true;
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested(true));
        }

        public void Execute(ShutdownCommand message)
        {
            Shutdown();
        }

        public void Execute(RestartCommand message)
        {
            Restart();
        }
    }
}
