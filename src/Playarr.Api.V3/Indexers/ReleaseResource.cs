using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Playarr.Core.DecisionEngine;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Indexers
{
    public class ReleaseResource : RestResource
    {
        public string Guid { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public int Age { get; set; }
        public double AgeHours { get; set; }
        public double AgeMinutes { get; set; }
        public long Size { get; set; }
        public int IndexerId { get; set; }
        public string Indexer { get; set; }
        public string ReleaseGroup { get; set; }
        public string SubGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Title { get; set; }
        public bool FullSeason { get; set; }
        public bool SceneSource { get; set; }
        public int PlatformNumber { get; set; }
        public List<Language> Languages { get; set; }
        public int LanguageWeight { get; set; }
        public string AirDate { get; set; }
        public string GameTitle { get; set; }
        public int[] RomNumbers { get; set; }
        public int[] AbsoluteRomNumbers { get; set; }
        public int? MappedPlatformNumber { get; set; }
        public int[] MappedRomNumbers { get; set; }
        public int[] MappedAbsoluteRomNumbers { get; set; }
        public int? MappedGameId { get; set; }
        public IEnumerable<ReleaseRomResource> MappedRomInfo { get; set; }
        public bool Approved { get; set; }
        public bool TemporarilyRejected { get; set; }
        public bool Rejected { get; set; }
        public int IgdbId { get; set; }
        public int MobyGamesId { get; set; }
        public string ImdbId { get; set; }
        public IEnumerable<string> Rejections { get; set; }
        public DateTime PublishDate { get; set; }
        public string CommentUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public bool EpisodeRequested { get; set; }
        public bool DownloadAllowed { get; set; }
        public int ReleaseWeight { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public AlternateTitleResource SceneMapping { get; set; }

        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public int IndexerFlags { get; set; }

        public bool IsDaily { get; set; }
        public bool IsAbsoluteNumbering { get; set; }
        public bool IsPossibleSpecialEpisode { get; set; }
        public bool Special { get; set; }

        // Sent when queuing an unknown release

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? GameId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? EpisodeId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<int> RomIds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? DownloadClientId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string DownloadClient { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? ShouldOverride { get; set; }
    }

    public static class ReleaseResourceMapper
    {
        public static ReleaseResource ToResource(this DownloadDecision model)
        {
            var releaseInfo = model.RemoteEpisode.Release;
            var parsedRomInfo = model.RemoteEpisode.ParsedRomInfo;
            var remoteRom = model.RemoteEpisode;
            var torrentInfo = (model.RemoteEpisode.Release as TorrentInfo) ?? new TorrentInfo();
            var indexerFlags = torrentInfo.IndexerFlags;

            return new ReleaseResource
            {
                Guid = releaseInfo.Guid,
                Quality = parsedRomInfo.Quality,

                // QualityWeight
                Age = releaseInfo.Age,
                AgeHours = releaseInfo.AgeHours,
                AgeMinutes = releaseInfo.AgeMinutes,
                Size = releaseInfo.Size,
                IndexerId = releaseInfo.IndexerId,
                Indexer = releaseInfo.Indexer,
                ReleaseGroup = parsedRomInfo.ReleaseGroup,
                ReleaseHash = parsedRomInfo.ReleaseHash,
                Title = releaseInfo.Title,
                FullSeason = parsedRomInfo.FullSeason,
                PlatformNumber = parsedRomInfo.PlatformNumber,
                Languages = remoteRom.Languages,
                AirDate = parsedRomInfo.AirDate,
                GameTitle = parsedRomInfo.GameTitle,
                RomNumbers = parsedRomInfo.RomNumbers,
                AbsoluteRomNumbers = parsedRomInfo.AbsoluteRomNumbers,
                MappedGameId = remoteRom.Game?.Id,
                MappedPlatformNumber = remoteRom.Roms.FirstOrDefault()?.PlatformNumber,
                MappedRomNumbers = remoteRom.Roms.Select(v => v.EpisodeNumber).ToArray(),
                MappedAbsoluteRomNumbers = remoteRom.Roms.Where(v => v.AbsoluteEpisodeNumber.HasValue).Select(v => v.AbsoluteEpisodeNumber.Value).ToArray(),
                MappedRomInfo = remoteRom.Roms.Select(v => new ReleaseRomResource(v)),
                Approved = model.Approved,
                TemporarilyRejected = model.TemporarilyRejected,
                Rejected = model.Rejected,
                IgdbId = releaseInfo.IgdbId,
                MobyGamesId = releaseInfo.MobyGamesId,
                ImdbId = releaseInfo.ImdbId,
                Rejections = model.Rejections.Select(r => r.Message).ToList(),
                PublishDate = releaseInfo.PublishDate,
                CommentUrl = releaseInfo.CommentUrl,
                DownloadUrl = releaseInfo.DownloadUrl,
                InfoUrl = releaseInfo.InfoUrl,
                EpisodeRequested = remoteRom.EpisodeRequested,
                DownloadAllowed = remoteRom.DownloadAllowed,

                // ReleaseWeight
                CustomFormatScore = remoteRom.CustomFormatScore,
                CustomFormats = remoteRom.CustomFormats?.ToResource(false),
                SceneMapping = remoteRom.SceneMapping.ToResource(),

                MagnetUrl = torrentInfo.MagnetUrl,
                InfoHash = torrentInfo.InfoHash,
                Seeders = torrentInfo.Seeders,
                Leechers = (torrentInfo.Peers.HasValue && torrentInfo.Seeders.HasValue) ? (torrentInfo.Peers.Value - torrentInfo.Seeders.Value) : (int?)null,
                Protocol = releaseInfo.DownloadProtocol,
                IndexerFlags = (int)indexerFlags,

                IsDaily = parsedRomInfo.IsDaily,
                IsAbsoluteNumbering = parsedRomInfo.IsAbsoluteNumbering,
                IsPossibleSpecialEpisode = parsedRomInfo.IsPossibleSpecialEpisode,
                Special = parsedRomInfo.Special,
            };
        }

        public static ReleaseInfo ToModel(this ReleaseResource resource)
        {
            ReleaseInfo model;

            if (resource.Protocol == DownloadProtocol.Torrent)
            {
                model = new TorrentInfo
                {
                    MagnetUrl = resource.MagnetUrl,
                    InfoHash = resource.InfoHash,
                    Seeders = resource.Seeders,
                    Peers = (resource.Seeders.HasValue && resource.Leechers.HasValue) ? (resource.Seeders + resource.Leechers) : null,
                    IndexerFlags = (IndexerFlags)resource.IndexerFlags
                };
            }
            else
            {
                model = new ReleaseInfo();
            }

            model.Guid = resource.Guid;
            model.Title = resource.Title;
            model.Size = resource.Size;
            model.DownloadUrl = resource.DownloadUrl;
            model.InfoUrl = resource.InfoUrl;
            model.CommentUrl = resource.CommentUrl;
            model.IndexerId = resource.IndexerId;
            model.Indexer = resource.Indexer;
            model.DownloadProtocol = resource.Protocol;
            model.IgdbId = resource.IgdbId;
            model.MobyGamesId = resource.MobyGamesId;
            model.ImdbId = resource.ImdbId;
            model.PublishDate = resource.PublishDate.ToUniversalTime();

            return model;
        }
    }

    public class ReleaseRomResource
    {
        public int Id { get; set; }
        public int PlatformNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public string Title { get; set; }

        public ReleaseRomResource()
        {
        }

        public ReleaseRomResource(Rom rom)
        {
            Id = rom.Id;
            PlatformNumber = rom.PlatformNumber;
            EpisodeNumber = rom.EpisodeNumber;
            AbsoluteEpisodeNumber = rom.AbsoluteEpisodeNumber;
            Title = rom.Title;
        }
    }
}
