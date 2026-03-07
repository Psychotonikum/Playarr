using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Blocklisting;
using Playarr.Core.History;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.CustomFormats
{
    public interface ICustomFormatCalculationService
    {
        List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteRom, long size);
        List<CustomFormat> ParseCustomFormat(RomFile romFile, Game game);
        List<CustomFormat> ParseCustomFormat(RomFile romFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Game game);
        List<CustomFormat> ParseCustomFormat(EpisodeHistory history, Game game);
        List<CustomFormat> ParseCustomFormat(LocalEpisode localRom, string fileName);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;
        private readonly Logger _logger;

        public CustomFormatCalculationService(ICustomFormatService formatService, Logger logger)
        {
            _formatService = formatService;
            _logger = logger;
        }

        public List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteRom, long size)
        {
            var input = new CustomFormatInput
            {
                RomInfo = remoteRom.ParsedRomInfo,
                Game = remoteRom.Game,
                Size = size,
                Languages = remoteRom.Languages,
                IndexerFlags = remoteRom.Release?.IndexerFlags ?? 0,
                ReleaseType = remoteRom.ParsedRomInfo.ReleaseType
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(RomFile romFile, Game game)
        {
            return ParseCustomFormat(romFile, game, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(RomFile romFile)
        {
            return ParseCustomFormat(romFile, romFile.Game.Value, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Game game)
        {
            var parsed = Parser.Parser.ParseTitle(blocklist.SourceTitle);

            var romInfo = new ParsedRomInfo
            {
                GameTitle = game.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? blocklist.SourceTitle,
                Quality = blocklist.Quality,
                Languages = blocklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                RomInfo = romInfo,
                Game = game,
                Size = blocklist.Size ?? 0,
                Languages = blocklist.Languages,
                IndexerFlags = blocklist.IndexerFlags,
                ReleaseType = blocklist.ReleaseType
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeHistory history, Game game)
        {
            var parsed = Parser.Parser.ParseTitle(history.SourceTitle);

            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);
            Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags indexerFlags);
            Enum.TryParse(history.Data.GetValueOrDefault("releaseType"), out ReleaseType releaseType);

            var romInfo = new ParsedRomInfo
            {
                GameTitle = game.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? history.SourceTitle,
                Quality = history.Quality,
                Languages = history.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
            };

            var input = new CustomFormatInput
            {
                RomInfo = romInfo,
                Game = game,
                Size = size,
                Languages = history.Languages,
                IndexerFlags = indexerFlags,
                ReleaseType = releaseType
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(LocalEpisode localRom, string fileName)
        {
            var romInfo = new ParsedRomInfo
            {
                GameTitle = localRom.Game.Title,
                ReleaseTitle = localRom.SceneName.IsNotNullOrWhiteSpace() ? localRom.SceneName : fileName,
                Quality = localRom.Quality,
                Languages = localRom.Languages,
                ReleaseGroup = localRom.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                RomInfo = romInfo,
                Game = localRom.Game,
                Size = localRom.Size,
                Languages = localRom.Languages,
                IndexerFlags = localRom.IndexerFlags,
                ReleaseType = localRom.ReleaseType,
                Filename = fileName
            };

            return ParseCustomFormat(input);
        }

        private List<CustomFormat> ParseCustomFormat(CustomFormatInput input)
        {
            return ParseCustomFormat(input, _formatService.All());
        }

        private static List<CustomFormat> ParseCustomFormat(CustomFormatInput input, List<CustomFormat> allCustomFormats)
        {
            var matches = new List<CustomFormat>();

            foreach (var customFormat in allCustomFormats)
            {
                var specificationMatches = customFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(input))
                    })
                    .ToList();

                if (specificationMatches.All(x => x.DidMatch))
                {
                    matches.Add(customFormat);
                }
            }

            return matches.OrderBy(x => x.Name).ToList();
        }

        private List<CustomFormat> ParseCustomFormat(RomFile romFile, Game game, List<CustomFormat> allCustomFormats)
        {
            var releaseTitle = string.Empty;

            if (romFile.SceneName.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using scene name for release title: {0}", romFile.SceneName);
                releaseTitle = romFile.SceneName;
            }
            else if (romFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using original file path for release title: {0}", Path.GetFileName(romFile.OriginalFilePath));
                releaseTitle = Path.GetFileName(romFile.OriginalFilePath);
            }
            else if (romFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using relative path for release title: {0}", Path.GetFileName(romFile.RelativePath));
                releaseTitle = Path.GetFileName(romFile.RelativePath);
            }

            var romInfo = new ParsedRomInfo
            {
                GameTitle = game.Title,
                ReleaseTitle = releaseTitle,
                Quality = romFile.Quality,
                Languages = romFile.Languages,
                ReleaseGroup = romFile.ReleaseGroup,
            };

            var input = new CustomFormatInput
            {
                RomInfo = romInfo,
                Game = game,
                Size = romFile.Size,
                Languages = romFile.Languages,
                IndexerFlags = romFile.IndexerFlags,
                ReleaseType = romFile.ReleaseType,
                Filename = Path.GetFileName(romFile.RelativePath),
            };

            return ParseCustomFormat(input, allCustomFormats);
        }
    }
}
