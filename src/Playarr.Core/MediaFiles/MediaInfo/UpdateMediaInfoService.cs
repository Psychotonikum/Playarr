using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.MediaInfo
{
    public interface IUpdateMediaInfo
    {
        bool Update(RomFile romFile, Game game);
        bool UpdateMediaInfo(RomFile romFile, Game game);
    }

    public class UpdateMediaInfoService : IUpdateMediaInfo, IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UpdateMediaInfoService(IDiskProvider diskProvider,
                                IMediaFileService mediaFileService,
                                IVideoFileInfoReader videoFileInfoReader,
                                IConfigService configService,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public void Handle(SeriesScannedEvent message)
        {
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return;
            }

            var allMediaFiles = _mediaFileService.GetFilesBySeries(message.Game.Id);
            var filteredMediaFiles = allMediaFiles.Where(c =>
                c.MediaInfo == null ||
                c.MediaInfo.SchemaRevision < VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION).ToList();

            foreach (var mediaFile in filteredMediaFiles)
            {
                UpdateMediaInfo(mediaFile, message.Game);
            }
        }

        public bool Update(RomFile romFile, Game game)
        {
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return false;
            }

            return UpdateMediaInfo(romFile, game);
        }

        public bool UpdateMediaInfo(RomFile romFile, Game game)
        {
            var path = romFile.Path.IsNotNullOrWhiteSpace() ? romFile.Path : Path.Combine(game.Path, romFile.RelativePath);

            if (!_diskProvider.FileExists(path))
            {
                _logger.Debug("Can't update MediaInfo because '{0}' does not exist", path);
                return false;
            }

            var updatedMediaInfo = _videoFileInfoReader.GetMediaInfo(path);

            if (updatedMediaInfo == null)
            {
                return false;
            }

            romFile.MediaInfo = updatedMediaInfo;

            if (romFile.Id != 0)
            {
                _mediaFileService.Update(romFile);
            }

            _logger.Debug("Updated MediaInfo for '{0}'", path);

            return true;
        }
    }
}
