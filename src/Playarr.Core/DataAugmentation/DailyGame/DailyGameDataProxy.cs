using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Cloud;
using Playarr.Common.Http;

namespace Playarr.Core.DataAugmentation.DailySeries
{
    public interface IDailySeriesDataProxy
    {
        IEnumerable<int> GetDailyGameIds();
    }

    public class DailySeriesDataProxy : IDailySeriesDataProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly Logger _logger;

        public DailySeriesDataProxy(IHttpClient httpClient, IPlayarrCloudRequestBuilder requestBuilder, Logger logger)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder.Services;
            _logger = logger;
        }

        public IEnumerable<int> GetDailyGameIds()
        {
            try
            {
                var dailySeriesRequest = _requestBuilder.Create()
                                                        .Resource("/dailyseries")
                                                        .Build();

                var response = _httpClient.Get<List<DailySeries>>(dailySeriesRequest);
                return response.Resource.Select(c => c.IgdbId);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get Daily Game");
                return Array.Empty<int>();
            }
        }
    }
}
