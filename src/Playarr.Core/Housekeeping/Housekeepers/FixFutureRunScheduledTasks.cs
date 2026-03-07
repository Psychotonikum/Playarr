using System;
using Dapper;
using NLog;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Datastore;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class FixFutureRunScheduledTasks : IHousekeepingTask
    {
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public FixFutureRunScheduledTasks(IMainDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            if (BuildInfo.IsDebug)
            {
                _logger.Debug("Not running scheduled task last execution cleanup during debug");
            }

            using var mapper = _database.OpenConnection();
            mapper.Execute(@"UPDATE ""ScheduledTasks""
                                 SET ""LastExecution"" = @time
                                 WHERE ""LastExecution"" > @time",
                           new { time = DateTime.UtcNow });
        }
    }
}
