using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using Playarr.Common;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Serializer;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Tags;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly Logger _logger;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly ITagRepository _tagRepo;
        private readonly IDetectXbmcNfo _detectNfo;
        private readonly IDiskProvider _diskProvider;

        public XbmcMetadata(IDetectXbmcNfo detectNfo,
                            IDiskProvider diskProvider,
                            IMapCoversToLocal mediaCoverService,
                            ITagRepository tagRepo,
                            Logger logger)
        {
            _logger = logger;
            _mediaCoverService = mediaCoverService;
            _tagRepo = tagRepo;
            _diskProvider = diskProvider;
            _detectNfo = detectNfo;
        }

        private static readonly Regex GameImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeasonImagesRegex = new Regex(@"^platform(?<platform>\d{2,}|-all|-specials)-(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EpisodeImageRegex = new Regex(@"-thumb\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Kodi (XBMC) / Emby";

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

            var metadata = new MetadataFile
            {
                GameId = game.Id,
                Consumer = GetType().Name,
                RelativePath = game.Path.GetRelativePath(path)
            };

            if (GameImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.GameImage;
                return metadata;
            }

            var seasonMatch = SeasonImagesRegex.Match(filename);

            if (seasonMatch.Success)
            {
                metadata.Type = MetadataType.SeasonImage;

                var platformNumberMatch = seasonMatch.Groups["platform"].Value;

                if (platformNumberMatch.Contains("specials"))
                {
                    metadata.PlatformNumber = 0;
                }
                else if (int.TryParse(platformNumberMatch, out var platformNumber))
                {
                    metadata.PlatformNumber = platformNumber;
                }
                else
                {
                    return null;
                }

                return metadata;
            }

            if (EpisodeImageRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.EpisodeImage;
                return metadata;
            }

            if (filename.Equals("tvshow.nfo", StringComparison.OrdinalIgnoreCase))
            {
                metadata.Type = MetadataType.SeriesMetadata;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseTitle(filename);

            if (parseResult != null &&
                !parseResult.FullSeason &&
                Path.GetExtension(filename).Equals(".nfo", StringComparison.OrdinalIgnoreCase) &&
                _detectNfo.IsXbmcNfoFile(path))
            {
                metadata.Type = MetadataType.EpisodeMetadata;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason)
        {
            if (reason == SeriesMetadataReason.EpisodesImported)
            {
                return null;
            }

            var xmlResult = string.Empty;

            if (Settings.SeriesMetadata)
            {
                _logger.Debug("Generating Game Metadata for: {0}", game.Title);

                var tvShow = new XElement("tvshow");

                tvShow.Add(new XElement("title", game.Title));

                if (game.Ratings != null && game.Ratings.Votes > 0)
                {
                    tvShow.Add(new XElement("rating", game.Ratings.Value));
                }

                tvShow.Add(new XElement("plot", game.Overview));
                tvShow.Add(new XElement("mpaa", game.Certification));
                tvShow.Add(new XElement("id", game.IgdbId));

                var uniqueId = new XElement("uniqueid", game.IgdbId);
                uniqueId.SetAttributeValue("type", "igdb");
                uniqueId.SetAttributeValue("default", true);
                tvShow.Add(uniqueId);

                if (game.ImdbId.IsNotNullOrWhiteSpace())
                {
                    var imdbId = new XElement("uniqueid", game.ImdbId);
                    imdbId.SetAttributeValue("type", "imdb");
                    tvShow.Add(imdbId);
                }

                if (game.TmdbId > 0)
                {
                    var tmdbId = new XElement("uniqueid", game.TmdbId);
                    tmdbId.SetAttributeValue("type", "tmdb");
                    tvShow.Add(tmdbId);
                }

                if (game.RawgId > 0)
                {
                    var rawgId = new XElement("uniqueid", game.RawgId);
                    rawgId.SetAttributeValue("type", "tvmaze");
                    tvShow.Add(rawgId);
                }

                foreach (var genre in game.Genres)
                {
                    tvShow.Add(new XElement("genre", genre));
                }

                if (game.Tags.Any())
                {
                    var tags = _tagRepo.GetTags(game.Tags);

                    foreach (var tag in tags)
                    {
                        tvShow.Add(new XElement("tag", tag.Label));
                    }
                }

                tvShow.Add(new XElement("status", game.Status));

                if (game.FirstAired.HasValue)
                {
                    tvShow.Add(new XElement("premiered", game.FirstAired.Value.ToString("yyyy-MM-dd")));
                }

                // Add support for Jellyfin's "enddate" tag
                if (game.Status == GameStatusType.Ended && game.LastAired.HasValue)
                {
                    tvShow.Add(new XElement("enddate", game.LastAired.Value.ToString("yyyy-MM-dd")));
                }

                tvShow.Add(new XElement("studio", game.Network));

                foreach (var actor in game.Actors)
                {
                    var xmlActor = new XElement("actor",
                        new XElement("name", actor.Name),
                        new XElement("role", actor.Character));

                    if (actor.Images.Any())
                    {
                        xmlActor.Add(new XElement("thumb", actor.Images.First().RemoteUrl));
                    }

                    tvShow.Add(xmlActor);
                }

                if (Settings.SeriesMetadataEpisodeGuide)
                {
                    var episodeGuide = new KodiEpisodeGuide(game);
                    var serializerSettings = STJson.GetSerializerSettings();
                    serializerSettings.WriteIndented = false;

                    tvShow.Add(new XElement("episodeguide", JsonSerializer.Serialize(episodeGuide, serializerSettings)));
                }

                var doc = new XDocument(tvShow)
                {
                    Declaration = new XDeclaration("1.0", "UTF-8", "yes"),
                };

                var sb = new StringBuilder();
                using var sw = new Utf8StringWriter();
                using var xw = XmlWriter.Create(sw, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true
                });

                doc.Save(xw);
                xw.Flush();

                xmlResult += sw.ToString();
            }

            if (Settings.SeriesMetadataUrl)
            {
                if (Settings.SeriesMetadata)
                {
                    xmlResult += Environment.NewLine;
                }

                xmlResult += "https://www.theigdb.com/?tab=game&id=" + game.IgdbId;
            }

            return xmlResult.IsNullOrWhiteSpace() ? null : new MetadataFileResult("tvshow.nfo", xmlResult);
        }

        public override MetadataFileResult EpisodeMetadata(Game game, RomFile romFile)
        {
            if (!Settings.EpisodeMetadata)
            {
                return null;
            }

            _logger.Debug("Generating Rom Metadata for: {0}", Path.Combine(game.Path, romFile.RelativePath));

            var watched = GetExistingWatchedStatus(game, romFile.RelativePath);

            var xws = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                ConformanceLevel =  ConformanceLevel.Fragment
            };

            using var sw = new Utf8StringWriter();
            using var xw = XmlWriter.Create(sw, xws);

            xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");

            foreach (var rom in romFile.Roms.Value)
            {
                var image = rom.Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

                var details = new XElement("episodedetails");
                details.Add(new XElement("title", rom.Title));
                details.Add(new XElement("platform", rom.PlatformNumber));
                details.Add(new XElement("rom", rom.EpisodeNumber));
                details.Add(new XElement("aired", rom.AirDate));
                details.Add(new XElement("plot", rom.Overview));

                if (rom.PlatformNumber == 0 && rom.AiredAfterPlatformNumber.HasValue)
                {
                    details.Add(new XElement("displayafterseason", rom.AiredAfterPlatformNumber));
                }
                else if (rom.PlatformNumber == 0 && rom.AiredBeforePlatformNumber.HasValue)
                {
                    details.Add(new XElement("displayseason", rom.AiredBeforePlatformNumber));
                    details.Add(new XElement("displayepisode", rom.AiredBeforeRomNumber ?? -1));
                }

                var igdbId = new XElement("uniqueid", rom.IgdbId);
                igdbId.SetAttributeValue("type", "igdb");
                igdbId.SetAttributeValue("default", true);
                details.Add(igdbId);

                var playarrId = new XElement("uniqueid", rom.Id);
                playarrId.SetAttributeValue("type", "playarr");
                details.Add(playarrId);

                if (image == null)
                {
                    details.Add(new XElement("thumb"));
                }
                else if (Settings.EpisodeImageThumb)
                {
                    details.Add(new XElement("thumb", image.RemoteUrl));
                }

                details.Add(new XElement("watched", watched));

                if (rom.Ratings != null && rom.Ratings.Votes > 0)
                {
                    details.Add(new XElement("rating", rom.Ratings.Value));
                }

                if (romFile.MediaInfo != null)
                {
                    var sceneName = romFile.GetSceneOrFileName();

                    var fileInfo = new XElement("fileinfo");
                    var streamDetails = new XElement("streamdetails");

                    var video = new XElement("video");
                    video.Add(new XElement("aspect", (float)romFile.MediaInfo.Width / (float)romFile.MediaInfo.Height));
                    video.Add(new XElement("bitrate", romFile.MediaInfo.VideoBitrate));
                    video.Add(new XElement("codec", MediaInfoFormatter.FormatVideoCodec(romFile.MediaInfo, sceneName)));
                    video.Add(new XElement("framerate", romFile.MediaInfo.VideoFps));
                    video.Add(new XElement("height", romFile.MediaInfo.Height));
                    video.Add(new XElement("scantype", romFile.MediaInfo.ScanType));
                    video.Add(new XElement("width", romFile.MediaInfo.Width));

                    video.Add(new XElement("duration", romFile.MediaInfo.RunTime.TotalMinutes));
                    video.Add(new XElement("durationinseconds", Math.Round(romFile.MediaInfo.RunTime.TotalSeconds)));

                    if (romFile.MediaInfo.VideoHdrFormat is HdrFormat.DolbyVision or HdrFormat.DolbyVisionHdr10 or HdrFormat.DolbyVisionHdr10Plus or HdrFormat.DolbyVisionHlg or HdrFormat.DolbyVisionSdr)
                    {
                        video.Add(new XElement("hdrtype", "dolbyvision"));
                    }
                    else if (romFile.MediaInfo.VideoHdrFormat is HdrFormat.Hdr10 or HdrFormat.Hdr10Plus or HdrFormat.Pq10)
                    {
                        video.Add(new XElement("hdrtype", "hdr10"));
                    }
                    else if (romFile.MediaInfo.VideoHdrFormat == HdrFormat.Hlg10)
                    {
                        video.Add(new XElement("hdrtype", "hlg"));
                    }
                    else if (romFile.MediaInfo.VideoHdrFormat == HdrFormat.None)
                    {
                        video.Add(new XElement("hdrtype", ""));
                    }

                    streamDetails.Add(video);

                    if (romFile.MediaInfo.AudioStreams is { Count: > 0 })
                    {
                        foreach (var audioStream in romFile.MediaInfo.AudioStreams)
                        {
                            var audio = new XElement("audio");
                            audio.Add(new XElement("bitrate", audioStream.Bitrate));
                            audio.Add(new XElement("channels", audioStream.Channels));
                            audio.Add(new XElement("codec", MediaInfoFormatter.FormatAudioCodec(audioStream, sceneName)));
                            audio.Add(new XElement("language", audioStream.Language));
                            streamDetails.Add(audio);
                        }
                    }

                    if (romFile.MediaInfo.SubtitleStreams is { Count: > 0 })
                    {
                        foreach (var subtitleStream in romFile.MediaInfo.SubtitleStreams)
                        {
                            var subtitle = new XElement("subtitle");
                            subtitle.Add(new XElement("language", subtitleStream.Language));
                            streamDetails.Add(subtitle);
                        }
                    }

                    fileInfo.Add(streamDetails);
                    details.Add(fileInfo);
                }

                // Todo: get guest stars, writer and director
                // details.Add(new XElement("credits", igdbEpisode.Writer.FirstOrDefault()));
                // details.Add(new XElement("director", igdbEpisode.Directors.FirstOrDefault()));

                details.WriteTo(xw);
            }

            xw.Flush();
            var xmlResult = sw.ToString();

            return new MetadataFileResult(GetEpisodeMetadataFilename(romFile.RelativePath), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> GameImages(Game game)
        {
            if (!Settings.GameImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessGameImages(game).ToList();
        }

        public override List<ImageFileResult> SeasonImages(Game game, Platform platform)
        {
            if (!Settings.SeasonImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessSeasonImages(game, platform).ToList();
        }

        public override List<ImageFileResult> EpisodeImages(Game game, RomFile romFile)
        {
            if (!Settings.EpisodeImages)
            {
                return new List<ImageFileResult>();
            }

            try
            {
                var firstEpisode = romFile.Roms.Value.FirstOrDefault();

                if (firstEpisode == null)
                {
                    _logger.Debug("Rom file has no associated roms, potentially a duplicate file");
                    return new List<ImageFileResult>();
                }

                var screenshot = firstEpisode.Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

                if (screenshot == null)
                {
                    _logger.Debug("Rom screenshot not available");
                    return new List<ImageFileResult>();
                }

                return new List<ImageFileResult>
                   {
                       new ImageFileResult(GetEpisodeImageFilename(romFile.RelativePath), screenshot.RemoteUrl)
                   };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to process rom image for file: {0}", Path.Combine(game.Path, romFile.RelativePath));

                return new List<ImageFileResult>();
            }
        }

        private IEnumerable<ImageFileResult> ProcessGameImages(Game game)
        {
            foreach (var image in game.Images)
            {
                var source = _mediaCoverService.GetCoverPath(game.Id, image.CoverType);
                var destination = image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source);

                yield return new ImageFileResult(destination, source);
            }
        }

        private IEnumerable<ImageFileResult> ProcessSeasonImages(Game game, Platform platform)
        {
            foreach (var image in platform.Images)
            {
                var filename = string.Format("platform{0:00}-{1}.jpg", platform.PlatformNumber, image.CoverType.ToString().ToLower());

                if (platform.PlatformNumber == 0)
                {
                    filename = string.Format("platform-specials-{0}.jpg", image.CoverType.ToString().ToLower());
                }

                yield return new ImageFileResult(filename, image.RemoteUrl);
            }
        }

        private string GetEpisodeMetadataFilename(string romFilePath)
        {
            return Path.ChangeExtension(romFilePath, "nfo");
        }

        private string GetEpisodeImageFilename(string romFilePath)
        {
            return Path.ChangeExtension(romFilePath, "").Trim('.') + "-thumb.jpg";
        }

        private bool GetExistingWatchedStatus(Game game, string romFilePath)
        {
            var fullPath = Path.Combine(game.Path, GetEpisodeMetadataFilename(romFilePath));

            if (!_diskProvider.FileExists(fullPath))
            {
                return false;
            }

            var fileContent = _diskProvider.ReadAllText(fullPath);

            return Regex.IsMatch(fileContent, "<watched>true</watched>");
        }
    }
}
