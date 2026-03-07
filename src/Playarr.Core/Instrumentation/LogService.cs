using Playarr.Core.Datastore;
using Playarr.Core.Instrumentation.Commands;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Instrumentation
{
    public interface ILogService
    {
        PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec);
    }

    public class LogService : ILogService, IExecute<ClearLogCommand>
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public PagingSpec<Log> Paged(PagingSpec<Log> pagingSpec)
        {
            return _logRepository.GetPaged(pagingSpec);
        }

        public void Execute(ClearLogCommand message)
        {
            _logRepository.Purge(vacuum: true);
        }
    }
}
