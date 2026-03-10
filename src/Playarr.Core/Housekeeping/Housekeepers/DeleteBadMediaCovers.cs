using System;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Core.Configuration;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.Games;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class DeleteBadMediaCovers : IHousekeepingTask
    {
        private readonly IMetadataFileService _metaFileService;
        private readonly IGameService _gameService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DeleteBadMediaCovers(IMetadataFileService metaFileService,
                                    IGameService seriesService,
                                    IDiskProvider diskProvider,
                                    IConfigService configService,
                                    Logger logger)
        {
            _metaFileService = metaFileService;
            _gameService = seriesService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public void Clean()
        {
            if (!_configService.CleanupMetadataImages)
            {
                return;
            }

            var game = _gameService.GetAllGamePaths();

            foreach (var show in game)
            {
                var images = _metaFileService.GetFilesBySeries(show.Key)
                    .Where(c => c.LastUpdated > new DateTime(2014, 12, 27) && c.RelativePath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase));

                foreach (var image in images)
                {
                    try
                    {
                        var path = Path.Combine(show.Value, image.RelativePath);
                        if (!IsValid(path))
                        {
                            _logger.Debug("Deleting invalid image file " + path);
                            DeleteMetadata(image.Id, path);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't validate image {0}", image.RelativePath);
                    }
                }
            }

            _configService.CleanupMetadataImages = false;
        }

        private void DeleteMetadata(int id, string path)
        {
            _metaFileService.Delete(id);
            _diskProvider.DeleteFile(path);
        }

        private bool IsValid(string path)
        {
            var buffer = new byte[10];

            using (var imageStream = _diskProvider.OpenReadStream(path))
            {
                if (imageStream.Length < buffer.Length)
                {
                    return false;
                }

                imageStream.Read(buffer, 0, buffer.Length);
            }

            var text = System.Text.Encoding.Default.GetString(buffer);

            return !text.ToLowerInvariant().Contains("html");
        }
    }
}
