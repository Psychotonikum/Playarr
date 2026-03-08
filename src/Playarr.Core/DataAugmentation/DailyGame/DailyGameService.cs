using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Cache;

namespace Playarr.Core.DataAugmentation.DailySeries
{
    public interface IDailyGameService
    {
        bool IsDailySeries(int igdbid);
    }

    public class DailyGameService : IDailyGameService
    {
        private readonly IDailySeriesDataProxy _proxy;
        private readonly ICached<List<int>> _cache;

        public DailyGameService(IDailySeriesDataProxy proxy, ICacheManager cacheManager)
        {
            _proxy = proxy;
            _cache = cacheManager.GetCache<List<int>>(GetType());
        }

        public bool IsDailySeries(int igdbid)
        {
            var dailySeries = _cache.Get("all", () => _proxy.GetDailyGameIds().ToList(), TimeSpan.FromHours(1));
            return dailySeries.Any(i => i == igdbid);
        }
    }
}
