using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxMetadata : MetadataBase<RoksboxMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RoksboxMetadata(IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex SeasonImagesRegex = new Regex(@"^(platform (?<platform>\d+))|(?<specials>specials)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static List<string> ValidCertification = new List<string> { "G", "NC-17", "PG", "PG-13", "R", "UR", "UNRATED", "NR", "TV-Y", "TV-Y7", "TV-Y7-FV", "TV-G", "TV-PG", "TV-14", "TV-MA" };

        public override string Name => "Roksbox";

        public override string GetFilenameAfterMove(Game game, RomFile romFile, MetadataFile metadataFile)
        {
            var romFilePath = Path.Combine(game.Path, romFile.RelativePath);

            if (metadataFile.Type == MetadataType.EpisodeImage)
            {
                return GetEpisodeImageFilename(romFilePath);
            }

            if (metadataFile.Type == MetadataType.EpisodeMetadata)
            {
                return GetEpisodeMetadataFilename(romFilePath);
            }

            _logger.Debug("Unknown rom file metadata: {0}", metadataFile.RelativePath);
            return Path.Combine(game.Path, metadataFile.RelativePath);
        }

        public override MetadataFile FindMetadataFile(Game game, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var parentdir = Directory.GetParent(path);

            var metadata = new MetadataFile
                           {
                               GameId = game.Id,
                               Consumer = GetType().Name,
                               RelativePath = game.Path.GetRelativePath(path)
                           };

            // Game and platform images are both named folder.jpg, only platform ones sit in platform folders
            if (Path.GetFileNameWithoutExtension(filename).Equals(parentdir.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var seasonMatch = SeasonImagesRegex.Match(parentdir.Name);

                if (seasonMatch.Success)
                {
                    metadata.Type = MetadataType.SeasonImage;

                    if (seasonMatch.Groups["specials"].Success)
                    {
                        metadata.PlatformNumber = 0;
                    }
                    else
                    {
                        metadata.PlatformNumber = Convert.ToInt32(seasonMatch.Groups["platform"].Value);
                    }

                    return metadata;
                }

                metadata.Type = MetadataType.GameImage;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseTitle(filename);

            if (parseResult != null &&
                !parseResult.FullSeason)
            {
                var extension = Path.GetExtension(filename).ToLowerInvariant();

                if (extension == ".xml")
                {
                    metadata.Type = MetadataType.EpisodeMetadata;
                    return metadata;
                }

                if (extension == ".jpg")
                {
                    if (!Path.GetFileNameWithoutExtension(filename).EndsWith("-thumb") &&
                        !path.GetParentName().Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                    {
                        metadata.Type = MetadataType.EpisodeImage;
                        return metadata;
                    }
                }
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason)
        {
            // Game metadata is not supported
            return null;
        }

        public override MetadataFileResult EpisodeMetadata(Game game, RomFile romFile)
        {
            if (!Settings.EpisodeMetadata)
            {
                return null;
            }

            _logger.Debug("Generating Rom Metadata for: {0}", romFile.RelativePath);

            var xmlResult = string.Empty;
            foreach (var rom in romFile.Roms.Value)
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();

                    var details = new XElement("video");
                    details.Add(new XElement("title", string.Format("{0} - {1}x{2} - {3}", game.Title, rom.PlatformNumber, rom.EpisodeNumber, rom.Title)));
                    details.Add(new XElement("year", rom.AirDate));
                    details.Add(new XElement("genre", string.Join(" / ", game.Genres)));
                    var actors = string.Join(" , ", game.Actors.ConvertAll(c => c.Name + " - " + c.Character).GetRange(0, Math.Min(3, game.Actors.Count)));
                    details.Add(new XElement("actors", actors));
                    details.Add(new XElement("description", rom.Overview));
                    details.Add(new XElement("length", game.Runtime));

                    if (game.Certification.IsNotNullOrWhiteSpace() &&
                        ValidCertification.Contains(game.Certification.ToUpperInvariant()))
                    {
                        details.Add(new XElement("mpaa", game.Certification.ToUpperInvariant()));
                    }
                    else
                    {
                        details.Add(new XElement("mpaa", "UNRATED"));
                    }

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            return new MetadataFileResult(GetEpisodeMetadataFilename(romFile.RelativePath), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            if (!Settings.GameImages)
            {
                return new List<ImageFileResult>();
            }

            var image = game.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? game.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Game image for game {0}.", game.Title);
                return new List<ImageFileResult>();
            }

            var source = _mediaCoverService.GetCoverPath(game.Id, image.CoverType);
            var destination = Path.GetFileName(game.Path) + Path.GetExtension(source);

            return new List<ImageFileResult> { new ImageFileResult(destination, source) };
        }

        public override List<ImageFileResult> SeasonImages(Game game, Platform platform)
        {
            if (!Settings.SeasonImages)
            {
                return new List<ImageFileResult>();
            }

            var platformFolders = GetPlatformFolders(game);

            if (!platformFolders.TryGetValue(platform.PlatformNumber, out var platformFolder))
            {
                _logger.Trace("Failed to find platform folder for game {0}, platform {1}.", game.Title, platform.PlatformNumber);
                return new List<ImageFileResult>();
            }

            // Roksbox only supports one platform image, so first of all try for poster otherwise just use whatever is first in the collection
            var image = platform.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? platform.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable platform image for game {0}, platform {1}.", game.Title, platform.PlatformNumber);
                return new List<ImageFileResult>();
            }

            var filename = Path.GetFileName(platformFolder) + ".jpg";
            var path = game.Path.GetRelativePath(Path.Combine(game.Path, platformFolder, filename));

            return new List<ImageFileResult> { new ImageFileResult(path, image.RemoteUrl) };
        }

        public override List<ImageFileResult> EpisodeImages(Game game, RomFile romFile)
        {
            if (!Settings.EpisodeImages)
            {
                return new List<ImageFileResult>();
            }

            var screenshot = romFile.Roms.Value.First().Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

            if (screenshot == null)
            {
                _logger.Trace("Rom screenshot not available");
                return new List<ImageFileResult>();
            }

            return new List<ImageFileResult> { new ImageFileResult(GetEpisodeImageFilename(romFile.RelativePath), screenshot.RemoteUrl) };
        }

        private string GetEpisodeMetadataFilename(string romFilePath)
        {
            return Path.ChangeExtension(romFilePath, "xml");
        }

        private string GetEpisodeImageFilename(string romFilePath)
        {
            return Path.ChangeExtension(romFilePath, "jpg");
        }

        private Dictionary<int, string> GetPlatformFolders(Game game)
        {
            var platformFolderMap = new Dictionary<int, string>();

            foreach (var folder in _diskProvider.GetDirectories(game.Path))
            {
                var directoryinfo = new DirectoryInfo(folder);
                var seasonMatch = SeasonImagesRegex.Match(directoryinfo.Name);

                if (seasonMatch.Success)
                {
                    var platformNumber = seasonMatch.Groups["platform"].Value;

                    if (platformNumber.Contains("specials"))
                    {
                        platformFolderMap[0] = folder;
                    }
                    else
                    {
                        if (int.TryParse(platformNumber, out var matchedSeason))
                        {
                            platformFolderMap[matchedSeason] = folder;
                        }
                        else
                        {
                            _logger.Debug("Failed to parse platform number from {0} for game {1}.", folder, game.Title);
                        }
                    }
                }
                else
                {
                    _logger.Debug("Rejecting folder {0} for game {1}.", Path.GetDirectoryName(folder), game.Title);
                }
            }

            return platformFolderMap;
        }
    }
}
