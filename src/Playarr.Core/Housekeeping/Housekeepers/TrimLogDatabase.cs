using Playarr.Core.Configuration;
using Playarr.Core.Instrumentation;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class TrimLogDatabase : IHousekeepingTask
    {
        private readonly ILogRepository _logRepo;
        private readonly IConfigFileProvider _configFileProvider;

        public TrimLogDatabase(ILogRepository logRepo, IConfigFileProvider configFileProvider)
        {
            _logRepo = logRepo;
            _configFileProvider = configFileProvider;
        }

        public void Clean()
        {
            if (!_configFileProvider.LogDbEnabled)
            {
                return;
            }

            _logRepo.Trim();
        }
    }
}
