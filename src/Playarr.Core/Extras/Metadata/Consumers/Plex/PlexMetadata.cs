using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadata : MetadataBase<PlexMetadataSettings>
    {
        private readonly IRomService _romService;
        private readonly IMediaFileService _mediaFileService;

        public PlexMetadata(IRomService episodeService, IMediaFileService mediaFileService)
        {
            _romService = episodeService;
            _mediaFileService = mediaFileService;
        }

        public override string Name => "Plex";

        public override MetadataFile FindMetadataFile(Game game, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var relativePath = game.Path.GetRelativePath(path);

            if (relativePath == ".plexmatch")
            {
                return new MetadataFile
                {
                    GameId = game.Id,
                    Consumer = GetType().Name,
                    RelativePath = game.Path.GetRelativePath(path),
                    Type = MetadataType.SeriesMetadata
                };
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason)
        {
            if (!Settings.SeriesPlexMatchFile)
            {
                return null;
            }

            var content = new StringBuilder();

            content.AppendLine($"Title: {game.Title}");
            content.AppendLine($"Year: {game.Year}");
            content.AppendLine($"IgdbId: {game.IgdbId}");
            content.AppendLine($"ImdbId: {game.ImdbId}");

            if (Settings.EpisodeMappings)
            {
                var roms = _romService.GetEpisodeBySeries(game.Id);
                var romFiles = _mediaFileService.GetFilesBySeries(game.Id);

                foreach (var romFile in romFiles)
                {
                    var episodesInFile = roms.Where(e => e.EpisodeFileId == romFile.Id);
                    var episodeFormat = $"S{romFile.PlatformNumber:00}{string.Join("-", episodesInFile.Select(e => $"E{e.EpisodeNumber:00}"))}";

                    if (romFile.PlatformNumber == 0)
                    {
                        episodeFormat = $"SP{episodesInFile.First().EpisodeNumber:00}";
                    }

                    content.AppendLine($"Rom: {episodeFormat}: {romFile.RelativePath}");
                }
            }

            return new MetadataFileResult(".plexmatch", content.ToString());
        }

        public override MetadataFileResult EpisodeMetadata(Game game, RomFile romFile)
        {
            return null;
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> SeasonImages(Game game, Platform platform)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> EpisodeImages(Game game, RomFile romFile)
        {
            return new List<ImageFileResult>();
        }
    }
}
