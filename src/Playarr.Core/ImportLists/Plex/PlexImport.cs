using System;
using System.Collections.Generic;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Exceptions;
using Playarr.Core.Localization;
using Playarr.Core.Notifications.Plex.PlexTv;
using Playarr.Core.Parser;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Plex
{
    public class PlexImport : HttpImportListBase<PlexListSettings>
    {
        public override string Name => _localizationService.GetLocalizedString("ImportListsPlexSettingsWatchlistName");
        public override ImportListType ListType => ImportListType.Plex;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override int PageSize => 100;
        public override TimeSpan RateLimit => TimeSpan.FromSeconds(5);

        private readonly IPlexTvService _plexTvService;

        public PlexImport(IPlexTvService plexTvService,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  ILocalizationService localizationService,
                                  Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
            _plexTvService = plexTvService;
        }

        public override ImportListFetchResult Fetch()
        {
            Settings.Validate().Filter("AccessToken").ThrowOnError();

            return FetchItems(g => g.GetListItems());
        }

        public override IParseImportListResponse GetParser()
        {
            return new PlexParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new PlexListRequestGenerator(_plexTvService, Settings, PageSize);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                return _plexTvService.GetPinUrl();
            }
            else if (action == "continueOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                if (query["id"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam id invalid.");
                }

                if (query["code"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam code invalid.");
                }

                return _plexTvService.GetSignInUrl(query["callbackUrl"], Convert.ToInt32(query["id"]), query["code"]);
            }
            else if (action == "pollOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var authToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                {
                    success = authToken.IsNotNullOrWhiteSpace()
                };
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var accessToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                {
                    accessToken
                };
            }

            return new { };
        }
    }
}
