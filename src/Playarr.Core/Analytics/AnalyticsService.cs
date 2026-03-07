using System;
using System.Linq;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Configuration;
using Playarr.Core.Datastore;
using Playarr.Core.History;

namespace Playarr.Core.Analytics
{
    public interface IAnalyticsService
    {
        bool IsEnabled { get; }
        bool InstallIsActive { get; }
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IHistoryService _historyService;

        public AnalyticsService(IHistoryService historyService, IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
            _historyService = historyService;
        }

        public bool IsEnabled => (_configFileProvider.AnalyticsEnabled && RuntimeInfo.IsProduction) || RuntimeInfo.IsDevelopment;

        public bool InstallIsActive
        {
            get
            {
                var lastRecord = _historyService.Paged(new PagingSpec<EpisodeHistory>() { Page = 0, PageSize = 1, SortKey = "date", SortDirection = SortDirection.Descending }, null, null);
                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                return lastRecord.Records.Any(v => v.Date > monthAgo);
            }
        }
    }
}
