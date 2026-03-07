using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.MediaFiles.Events;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public interface IUpdateRomFileService
    {
        void ChangeFileDateForFile(RomFile romFile, Game game, List<Rom> roms);
    }

    public class UpdateRomFileService : IUpdateRomFileService,
                                            IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IRomService _episodeService;
        private readonly Logger _logger;

        public UpdateRomFileService(IDiskProvider diskProvider,
                                        IConfigService configService,
                                        IRomService episodeService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void ChangeFileDateForFile(RomFile romFile, Game game, List<Rom> roms)
        {
            ChangeFileDate(romFile, game, roms);
        }

        private bool ChangeFileDate(RomFile romFile, Game game, List<Rom> roms)
        {
            var romFilePath = Path.Combine(game.Path, romFile.RelativePath);
            var airDateUtc = roms.First().AirDateUtc;

            if (!airDateUtc.HasValue)
            {
                return false;
            }

            return _configService.FileDate switch
            {
                FileDateType.LocalAirDate =>
                    ChangeFileDateToLocalDate(romFilePath, airDateUtc.Value.ToLocalTime()),

                // Intentionally pass UTC as local per user preference
                FileDateType.UtcAirDate =>
                    ChangeFileDateToLocalDate(
                        romFilePath,
                        DateTime.SpecifyKind(airDateUtc.Value, DateTimeKind.Local)),

                _ => false,
            };
        }

        private bool ChangeFileDateToLocalDate(string filePath, DateTime localDate)
        {
            // FileGetLastWrite returns UTC; convert to local to compare
            var oldLastWrite = _diskProvider.FileGetLastWrite(filePath).ToLocalTime();

            if (OsInfo.IsNotWindows && localDate.ToUniversalTime() < DateTimeExtensions.EpochTime)
            {
                _logger.Debug("Setting date of file to 1970-01-01 as actual airdate is before that time and will not be set properly");
                localDate = DateTimeExtensions.EpochTime.ToLocalTime();
            }

            if (!DateTime.Equals(localDate.WithoutTicks(), oldLastWrite.WithoutTicks()))
            {
                try
                {
                    // Preserve prior mtime subseconds per https://github.com/Playarr/Playarr/issues/7228
                    var mtime = localDate.WithTicksFrom(oldLastWrite);

                    _diskProvider.FileSetLastWriteTime(filePath, mtime);
                    _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldLastWrite, mtime);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                }
            }

            return false;
        }

        public void Handle(SeriesScannedEvent message)
        {
            if (_configService.FileDate == FileDateType.None)
            {
                return;
            }

            var roms = _episodeService.EpisodesWithFiles(message.Game.Id);

            var romFiles = new List<RomFile>();
            var updated = new List<RomFile>();

            foreach (var group in roms.GroupBy(e => e.EpisodeFileId))
            {
                var episodesInFile = group.Select(e => e).ToList();
                var romFile = episodesInFile.First().RomFile;

                romFiles.Add(romFile);

                if (ChangeFileDate(romFile, message.Game, episodesInFile))
                {
                    updated.Add(romFile);
                }
            }

            if (updated.Any())
            {
                _logger.ProgressDebug("Changed file date for {0} files of {1} in {2}", updated.Count, romFiles.Count, message.Game.Title);
            }
            else
            {
                _logger.ProgressDebug("No file dates changed for {0}", message.Game.Title);
            }
        }
    }
}
