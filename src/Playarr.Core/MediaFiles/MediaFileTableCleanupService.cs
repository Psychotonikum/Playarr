using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Playarr.Common;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Game game, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRomService _romService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IRomService episodeService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _romService = episodeService;
            _logger = logger;
        }

        public void Clean(Game game, List<string> filesOnDisk)
        {
            var seriesFiles = _mediaFileService.GetFilesBySeries(game.Id);
            var roms = _romService.GetEpisodeBySeries(game.Id);

            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);

            foreach (var seriesFile in seriesFiles)
            {
                var romFile = seriesFile;
                var romFilePath = Path.Combine(game.Path, romFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(romFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", romFilePath);
                        _mediaFileService.Delete(seriesFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }

                    if (roms.None(e => e.EpisodeFileId == romFile.Id))
                    {
                        _logger.Debug("File [{0}] is not assigned to any roms, removing from db", romFilePath);
                        _mediaFileService.Delete(romFile, DeleteMediaFileReason.NoLinkedEpisodes);
                        continue;
                    }

// var localEpsiode = _parsingService.GetLocalEpisode(romFile.Path, game);
//
//                    if (localEpsiode == null || roms.Count != localEpsiode.Roms.Count)
//                    {
//                        _logger.Debug("File [{0}] parsed roms has changed, removing from db", romFile.Path);
//                        _mediaFileService.Delete(romFile);
//                        continue;
//                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to cleanup RomFile in DB: {0}", romFile.Id);
                }
            }

            foreach (var e in roms)
            {
                var rom = e;

                if (rom.EpisodeFileId > 0 && seriesFiles.None(f => f.Id == rom.EpisodeFileId))
                {
                    rom.EpisodeFileId = 0;
                    _romService.UpdateEpisode(rom);
                }
            }
        }
    }
}
