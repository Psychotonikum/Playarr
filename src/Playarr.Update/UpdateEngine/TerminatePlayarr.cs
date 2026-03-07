using System;
using NLog;
using Playarr.Common;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Processes;
using IServiceProvider = Playarr.Common.IServiceProvider;

namespace Playarr.Update.UpdateEngine
{
    public interface ITerminatePlayarr
    {
        void Terminate(int processId);
    }

    public class TerminatePlayarr : ITerminatePlayarr
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public TerminatePlayarr(IServiceProvider serviceProvider, IProcessProvider processProvider, Logger logger)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Terminate(int processId)
        {
            if (OsInfo.IsWindows)
            {
                _logger.Info("Stopping all running services");

                if (_serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME)
                    && _serviceProvider.IsServiceRunning(ServiceProvider.SERVICE_NAME))
                {
                    try
                    {
                        _logger.Info("Playarr Service is installed and running");
                        _serviceProvider.Stop(ServiceProvider.SERVICE_NAME);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "couldn't stop service");
                    }
                }

                _logger.Info("Killing all running processes");

                _processProvider.KillAll(ProcessProvider.PLAYARR_CONSOLE_PROCESS_NAME);
                _processProvider.KillAll(ProcessProvider.PLAYARR_PROCESS_NAME);
            }
            else
            {
                _logger.Info("Killing all running processes");

                _processProvider.KillAll(ProcessProvider.PLAYARR_CONSOLE_PROCESS_NAME);
                _processProvider.KillAll(ProcessProvider.PLAYARR_PROCESS_NAME);

                _processProvider.Kill(processId);
            }
        }
    }
}
