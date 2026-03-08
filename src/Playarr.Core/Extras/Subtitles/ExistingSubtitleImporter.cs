using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Files;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Subtitles
{
    public class ExistingSubtitleImporter : ImportExistingExtraFilesBase<SubtitleFile>
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IAggregationService _aggregationService;
        private readonly Logger _logger;

        public ExistingSubtitleImporter(IExtraFileService<SubtitleFile> subtitleFileService,
                                        IAggregationService aggregationService,
                                        Logger logger)
            : base(subtitleFileService)
        {
            _subtitleFileService = subtitleFileService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename)
        {
            _logger.Debug("Looking for existing subtitle files in {0}", game.Path);

            var subtitleFiles = new List<SubtitleFile>();
            var filterResult = FilterAndClean(game, filesOnDisk, importedFiles, fileNameBeforeRename is not null);

            foreach (var possibleSubtitleFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleSubtitleFile);

                if (SubtitleFileExtensions.Extensions.Contains(extension))
                {
                    var localRom = new LocalEpisode
                    {
                        FileRomInfo = Parser.Parser.ParsePath(possibleSubtitleFile),
                        Game = game,
                        Path = possibleSubtitleFile,
                        FileNameBeforeRename = fileNameBeforeRename
                    };

                    try
                    {
                        _aggregationService.Augment(localRom, null);
                    }
                    catch (AugmentingFailedException)
                    {
                        _logger.Debug("Unable to parse extra file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localRom.Roms.Empty())
                    {
                        _logger.Debug("Cannot find related roms for: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localRom.Roms.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                    {
                        _logger.Debug("Subtitle file: {0} does not match existing files.", possibleSubtitleFile);
                        continue;
                    }

                    var firstEpisode = localRom.Roms.First();

                    var subtitleFile = new SubtitleFile
                                       {
                                           GameId = game.Id,
                                           PlatformNumber = localRom.PlatformNumber,
                                           EpisodeFileId = firstEpisode.EpisodeFileId,
                                           RelativePath = game.Path.GetRelativePath(possibleSubtitleFile),
                                           Language = localRom.SubtitleInfo?.Language ?? Language.Unknown,
                                           LanguageTags = localRom.SubtitleInfo?.LanguageTags ?? new List<string>(),
                                           Title = localRom.SubtitleInfo?.Title,
                                           Extension = extension,
                                           Copy = localRom.SubtitleInfo?.Copy ?? 0
                                       };

                    subtitleFiles.Add(subtitleFile);
                }
            }

            _logger.Info("Found {0} existing subtitle files", subtitleFiles.Count);
            _subtitleFileService.Upsert(subtitleFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return subtitleFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
