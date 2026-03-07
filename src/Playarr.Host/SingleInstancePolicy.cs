using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Processes;

namespace Playarr.Host
{
    public interface ISingleInstancePolicy
    {
        void PreventStartIfAlreadyRunning();
        void KillAllOtherInstance();
        void WarnIfAlreadyRunning();
    }

    public class SingleInstancePolicy : ISingleInstancePolicy
    {
        private readonly IProcessProvider _processProvider;
        private readonly IBrowserService _browserService;
        private readonly Logger _logger;

        public SingleInstancePolicy(IProcessProvider processProvider,
                                    IBrowserService browserService,
                                    Logger logger)
        {
            _processProvider = processProvider;
            _browserService = browserService;
            _logger = logger;
        }

        public void PreventStartIfAlreadyRunning()
        {
            if (IsAlreadyRunning())
            {
                _logger.Warn("Another instance of Playarr is already running.");
                _browserService.LaunchWebUI();
                throw new TerminateApplicationException("Another instance is already running");
            }
        }

        public void KillAllOtherInstance()
        {
            foreach (var processId in GetOtherPlayarrProcessIds())
            {
                _processProvider.Kill(processId);
            }
        }

        public void WarnIfAlreadyRunning()
        {
            if (IsAlreadyRunning())
            {
                _logger.Debug("Another instance of Playarr is already running.");
            }
        }

        private bool IsAlreadyRunning()
        {
            return GetOtherPlayarrProcessIds().Any();
        }

        private List<int> GetOtherPlayarrProcessIds()
        {
            try
            {
                var currentId = _processProvider.GetCurrentProcess().Id;

                var otherProcesses = _processProvider.FindProcessByName(ProcessProvider.PLAYARR_CONSOLE_PROCESS_NAME)
                                                     .Union(_processProvider.FindProcessByName(ProcessProvider.PLAYARR_PROCESS_NAME))
                                                     .Select(c => c.Id)
                                                     .Except(new[] { currentId })
                                                     .ToList();

                if (otherProcesses.Any())
                {
                    _logger.Info("{0} instance(s) of Playarr are running", otherProcesses.Count);
                }

                return otherProcesses;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to check for multiple instances of Playarr.");
                return new List<int>();
            }
        }
    }
}
