using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.Download.Aggregation.Aggregators
{
    public class AggregateLanguages : IAggregateRemoteEpisode
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public AggregateLanguages(IIndexerFactory indexerFactory,
                                  Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public RemoteRom Aggregate(RemoteRom remoteRom)
        {
            var parsedRomInfo = remoteRom.ParsedRomInfo;
            var releaseInfo = remoteRom.Release;
            var languages = parsedRomInfo.Languages;
            var game = remoteRom.Game;
            var releaseTokens = parsedRomInfo.ReleaseTokens ?? parsedRomInfo.ReleaseTitle;
            var normalizedReleaseTokens = Parser.Parser.NormalizeRomTitle(releaseTokens);
            var languagesToRemove = new List<Language>();

            if (game == null)
            {
                _logger.Debug("Unable to aggregate languages, using parsed values: {0}", string.Join(", ", languages.ToList()));

                remoteRom.Languages = releaseInfo != null && releaseInfo.Languages.Any() ? releaseInfo.Languages : languages;

                return remoteRom;
            }

            if (releaseInfo != null && releaseInfo.Languages.Any())
            {
                _logger.Debug("Languages provided by indexer, using release values: {0}", string.Join(", ", releaseInfo.Languages));

                // Use languages from release (given by indexer or user) if available
                languages = releaseInfo.Languages;
            }
            else
            {
                // Exclude any languages that are part of the rom title, if the rom title is in the release tokens (falls back to release title)
                foreach (var rom in remoteRom.Roms)
                {
                    var romTitleLanguage = LanguageParser.ParseLanguages(rom.Title);

                    if (!romTitleLanguage.Contains(Language.Unknown))
                    {
                        var normalizedRomTitle = Parser.Parser.NormalizeRomTitle(rom.Title);
                        var romTitleIndex = normalizedReleaseTokens.IndexOf(normalizedRomTitle,
                            StringComparison.CurrentCultureIgnoreCase);

                        if (romTitleIndex >= 0)
                        {
                            releaseTokens = releaseTokens.Remove(romTitleIndex, normalizedRomTitle.Length);
                            languagesToRemove.AddRange(romTitleLanguage);
                        }
                    }
                }

                // Remove any languages still in the title that would normally be removed
                languagesToRemove = languagesToRemove.Except(LanguageParser.ParseLanguages(releaseTokens)).ToList();

                // Remove all languages that aren't part of the updated releaseTokens
                languages = languages.Except(languagesToRemove).ToList();
            }

            if (releaseInfo?.Title?.IsNotNullOrWhiteSpace() == true)
            {
                IndexerDefinition indexer = null;

                if (releaseInfo is { IndexerId: > 0 })
                {
                    indexer = _indexerFactory.Find(releaseInfo.IndexerId);
                }

                if (indexer == null && releaseInfo.Indexer?.IsNotNullOrWhiteSpace() == true)
                {
                    indexer = _indexerFactory.FindByName(releaseInfo.Indexer);
                }

                if (indexer?.Settings is IIndexerSettings settings && settings.MultiLanguages.Any() && Parser.Parser.HasMultipleLanguages(releaseInfo.Title))
                {
                    // Use indexer setting for Multi-languages
                    if (languages.Count == 0 || (languages.Count == 1 && languages.First() == Language.Unknown))
                    {
                        languages = settings.MultiLanguages.Select(i => (Language)i).ToList();
                    }
                    else
                    {
                        languages.AddRange(settings.MultiLanguages.Select(i => (Language)i).Except(languages).ToList());
                    }
                }
            }

            // Use game language as fallback if we couldn't parse a language
            if (languages.Count == 0 || (languages.Count == 1 && languages.First() == Language.Unknown))
            {
                languages = new List<Language> { game.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, fallback to game original language: {0}", game.OriginalLanguage.Name);
            }

            if (languages.Contains(Language.Original))
            {
                languages.Remove(Language.Original);

                if (!languages.Contains(game.OriginalLanguage))
                {
                    languages.Add(game.OriginalLanguage);
                }
                else
                {
                    languages.Add(Language.Unknown);
                }
            }

            _logger.Debug("Selected languages: {0}", string.Join(", ", languages.ToList()));

            remoteRom.Languages = languages;

            return remoteRom;
        }
    }
}
