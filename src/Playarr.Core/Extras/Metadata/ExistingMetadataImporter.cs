using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Files;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.Extras.Subtitles;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata
{
    public class ExistingMetadataImporter : ImportExistingExtraFilesBase<MetadataFile>
    {
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IAggregationService _aggregationService;
        private readonly Logger _logger;
        private readonly List<IMetadata> _consumers;

        public ExistingMetadataImporter(IExtraFileService<MetadataFile> metadataFileService,
                                        IEnumerable<IMetadata> consumers,
                                        IAggregationService aggregationService,
                                        Logger logger)
        : base(metadataFileService)
        {
            _metadataFileService = metadataFileService;
            _aggregationService = aggregationService;
            _logger = logger;
            _consumers = consumers.ToList();
        }

        public override int Order => 0;

        public override IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename)
        {
            _logger.Debug("Looking for existing metadata in {0}", game.Path);

            var metadataFiles = new List<MetadataFile>();
            var filterResult = FilterAndClean(game, filesOnDisk, importedFiles, fileNameBeforeRename is not null);

            foreach (var possibleMetadataFile in filterResult.FilesOnDisk)
            {
                // Don't process files that have known Subtitle file extensions (saves a bit of unnecessary processing)

                if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(possibleMetadataFile)))
                {
                    continue;
                }

                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(game, possibleMetadataFile);

                    if (metadata == null)
                    {
                        continue;
                    }

                    if (metadata.Type == MetadataType.EpisodeImage ||
                        metadata.Type == MetadataType.EpisodeMetadata)
                    {
                        var localRom = new LocalEpisode
                        {
                            FileRomInfo = Parser.Parser.ParsePath(possibleMetadataFile),
                            Game = game,
                            Path = possibleMetadataFile
                        };

                        try
                        {
                            _aggregationService.Augment(localRom, null);
                        }
                        catch (AugmentingFailedException)
                        {
                            _logger.Debug("Unable to parse extra file: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localRom.Roms.Empty())
                        {
                            _logger.Debug("Cannot find related roms for: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localRom.Roms.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                        {
                            _logger.Debug("Extra file: {0} does not match existing files.", possibleMetadataFile);
                            continue;
                        }

                        metadata.PlatformNumber = localRom.PlatformNumber;
                        metadata.EpisodeFileId = localRom.Roms.First().EpisodeFileId;
                    }

                    metadata.Extension = Path.GetExtension(possibleMetadataFile);

                    metadataFiles.Add(metadata);
                }
            }

            _logger.Info("Found {0} existing metadata files", metadataFiles.Count);
            _metadataFileService.Upsert(metadataFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return metadataFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
