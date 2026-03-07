using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.Download;
using Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation
{
    public interface IAggregationService
    {
        LocalEpisode Augment(LocalEpisode localRom, DownloadClientItem downloadClientItem);
    }

    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IAggregateLocalEpisode> _augmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AggregationService(IEnumerable<IAggregateLocalEpisode> augmenters,
                                 IDiskProvider diskProvider,
                                 IVideoFileInfoReader videoFileInfoReader,
                                 IConfigService configService,
                                 Logger logger)
        {
            _augmenters = augmenters.OrderBy(a => a.Order).ToList();
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public LocalEpisode Augment(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localRom.Path));

            if (localRom.DownloadClientRomInfo == null &&
                localRom.FolderRomInfo == null &&
                localRom.FileRomInfo == null)
            {
                if (isMediaFile)
                {
                    throw new AugmentingFailedException("Unable to parse rom info from path: {0}", localRom.Path);
                }
            }

            localRom.Size = _diskProvider.GetFileSize(localRom.Path);
            localRom.SceneName = localRom.SceneSource ? SceneNameCalculator.GetSceneName(localRom) : null;

            if (isMediaFile && (!localRom.ExistingFile || _configService.EnableMediaInfo))
            {
                localRom.MediaInfo = _videoFileInfoReader.GetMediaInfo(localRom.Path);
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(localRom, downloadClientItem);
                }
                catch (Exception ex)
                {
                    var message = $"Unable to augment information for file: '{localRom.Path}'. Game: {localRom.Game} Error: {ex.Message}";

                    _logger.Warn(ex, message);
                }
            }

            return localRom;
        }
    }
}
