using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata.Consumers.Batocera
{
    public class BatoceraMetadata : MetadataBase<BatoceraMetadataSettings>
    {
        private readonly Logger _logger;
        private readonly IMapCoversToLocal _mediaCoverService;

        public BatoceraMetadata(IMapCoversToLocal mediaCoverService, Logger logger)
        {
            _logger = logger;
            _mediaCoverService = mediaCoverService;
        }

        public override string Name => "Batocera / RetroArch";

        private static readonly Regex GameImageRegex = new Regex(@"^images[/\\].+\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override MetadataFile FindMetadataFile(Game game, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var relativePath = game.Path.GetRelativePath(path);

            if (filename.Equals("gamelist.xml", StringComparison.OrdinalIgnoreCase))
            {
                return new MetadataFile
                {
                    GameId = game.Id,
                    Consumer = GetType().Name,
                    RelativePath = relativePath,
                    Type = MetadataType.SeriesMetadata
                };
            }

            if (GameImageRegex.IsMatch(relativePath))
            {
                return new MetadataFile
                {
                    GameId = game.Id,
                    Consumer = GetType().Name,
                    RelativePath = relativePath,
                    Type = MetadataType.GameImage
                };
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason)
        {
            if (!Settings.GamelistXml)
            {
                return null;
            }

            _logger.Debug("Generating Batocera gamelist.xml for: {0}", game.Title);

            var gamelistXml = new XElement("gameList");
            var gameElement = new XElement("game");

            gameElement.Add(new XElement("name", game.Title));
            gameElement.Add(new XElement("desc", game.Overview ?? string.Empty));

            if (game.Ratings != null && game.Ratings.Votes > 0)
            {
                var normalizedRating = Math.Clamp(game.Ratings.Value / 100.0m, 0m, 1m);
                gameElement.Add(new XElement("rating", normalizedRating.ToString("F2")));
            }

            if (game.FirstAired.HasValue)
            {
                gameElement.Add(new XElement("releasedate", game.FirstAired.Value.ToString("yyyyMMddT000000")));
            }

            gameElement.Add(new XElement("developer", game.Network ?? string.Empty));

            if (game.Genres != null && game.Genres.Any())
            {
                gameElement.Add(new XElement("genre", string.Join(", ", game.Genres)));
            }

            gameElement.Add(new XElement("players", "1"));

            if (Settings.GameImages)
            {
                var covers = _mediaCoverService.GetCoverPath(game.Id, MediaCoverTypes.Poster);

                if (covers.IsNotNullOrWhiteSpace())
                {
                    gameElement.Add(new XElement("image", $"./images/{game.Title}-image.jpg"));
                }
            }

            gamelistXml.Add(gameElement);

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new System.Text.UTF8Encoding(false)
            };

            using var stringWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings))
            {
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), gamelistXml);
                doc.WriteTo(xmlWriter);
            }

            return new MetadataFileResult("gamelist.xml", stringWriter.ToString().Trim());
        }

        public override MetadataFileResult EpisodeMetadata(Game game, RomFile romFile)
        {
            return null;
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            if (!Settings.GameImages)
            {
                return new List<ImageFileResult>();
            }

            var images = new List<ImageFileResult>();
            var covers = _mediaCoverService.GetCoverPath(game.Id, MediaCoverTypes.Poster);

            if (covers.IsNotNullOrWhiteSpace())
            {
                images.Add(new ImageFileResult(Path.Combine("images", $"{game.Title}-image.jpg"), covers));
            }

            return images;
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
