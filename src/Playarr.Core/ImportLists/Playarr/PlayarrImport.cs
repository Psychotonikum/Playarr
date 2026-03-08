using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.ImportLists.Playarr
{
    public class PlayarrImport : ImportListBase<PlayarrSettings>
    {
        private readonly IPlayarrV3Proxy _playarrV3Proxy;
        public override string Name => "Playarr";

        public override ImportListType ListType => ImportListType.Program;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromMinutes(5);

        public PlayarrImport(IPlayarrV3Proxy playarrV3Proxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, localizationService, logger)
        {
            _playarrV3Proxy = playarrV3Proxy;
        }

        public override ImportListFetchResult Fetch()
        {
            var game = new List<ImportListItemInfo>();
            var anyFailure = false;
            try
            {
                var remoteGame = _playarrV3Proxy.GetSeries(Settings);

                foreach (var item in remoteGame)
                {
                    if (Settings.ProfileIds.Any() && !Settings.ProfileIds.Contains(item.QualityProfileId))
                    {
                        continue;
                    }

                    if (Settings.LanguageProfileIds.Any() && !Settings.LanguageProfileIds.Contains(item.LanguageProfileId))
                    {
                        continue;
                    }

                    if (Settings.TagIds.Any() && !Settings.TagIds.Any(tagId => item.Tags.Any(itemTagId => itemTagId == tagId)))
                    {
                        continue;
                    }

                    if (Settings.RootFolderPaths.Any() && !Settings.RootFolderPaths.Any(rootFolderPath => item.RootFolderPath.ContainsIgnoreCase(rootFolderPath)))
                    {
                        continue;
                    }

                    var info = new ImportListItemInfo
                    {
                        IgdbId = item.IgdbId,
                        Title = item.Title
                    };

                    if (Settings.SyncSeasonMonitoring)
                    {
                        info.Platforms = item.Platforms.Select(s => new Platform
                        {
                            PlatformNumber = s.PlatformNumber,
                            Monitored = s.Monitored
                        }).ToList();
                    }

                    game.Add(info);
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to fetch data for list {0} ({1})", Definition.Name, Name);

                _importListStatusService.RecordFailure(Definition.Id);
                anyFailure = true;
            }

            return new ImportListFetchResult(CleanupListItems(game), anyFailure);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            // Return early if there is not an API key
            if (Settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new
                {
                    devices = new List<object>()
                };
            }

            Settings.Validate().Filter("ApiKey").ThrowOnError();

            if (action == "getProfiles")
            {
                var profiles = _playarrV3Proxy.GetQualityProfiles(Settings);

                return new
                {
                    options = profiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Name
                        })
                };
            }

            if (action == "getLanguageProfiles")
            {
                var langProfiles = _playarrV3Proxy.GetLanguageProfiles(Settings);

                return new
                {
                    options = langProfiles.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Name
                        })
                };
            }

            if (action == "getTags")
            {
                var tags = _playarrV3Proxy.GetTags(Settings);

                return new
                {
                    options = tags.OrderBy(d => d.Label, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Id,
                            name = d.Label
                        })
                };
            }

            if (action == "getRootFolders")
            {
                var remoteRootFolders = _playarrV3Proxy.GetRootFolders(Settings);

                return new
                {
                    options = remoteRootFolders.OrderBy(d => d.Path, StringComparer.InvariantCultureIgnoreCase)
                        .Select(d => new
                        {
                            value = d.Path,
                            name = d.Path
                        })
                };
            }

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_playarrV3Proxy.Test(Settings));
        }
    }
}
