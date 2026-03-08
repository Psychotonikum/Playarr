using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.ImportLists.Custom
{
    public class CustomImport : ImportListBase<CustomSettings>
    {
        private readonly ICustomImportProxy _customProxy;
        public override string Name => _localizationService.GetLocalizedString("ImportListsCustomListSettingsName");

        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override ImportListType ListType => ImportListType.Advanced;

        public CustomImport(ICustomImportProxy customProxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, localizationService, logger)
        {
            _customProxy = customProxy;
        }

        public override ImportListFetchResult Fetch()
        {
            var game = new List<ImportListItemInfo>();
            var anyFailure = false;

            try
            {
                var remoteGame = _customProxy.GetSeries(Settings);

                foreach (var item in remoteGame)
                {
                    game.Add(new ImportListItemInfo
                    {
                        Title = item.Title.IsNullOrWhiteSpace() ? $"IgdbId: {item.IgdbId}" : item.Title,
                        IgdbId = item.IgdbId,
                        TmdbId = item.TmdbId,
                        ImdbId = item.ImdbId
                    });
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch (Exception ex)
            {
                anyFailure = true;
                _logger.Debug(ex, "Failed to fetch data for list {0} ({1})", Definition.Name, Name);

                _importListStatusService.RecordFailure(Definition.Id);
            }

            return new ImportListFetchResult(CleanupListItems(game), anyFailure);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_customProxy.Test(Settings));
        }
    }
}
