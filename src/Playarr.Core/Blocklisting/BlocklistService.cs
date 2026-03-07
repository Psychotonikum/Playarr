using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Download;
using Playarr.Core.Indexers;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games.Events;

namespace Playarr.Core.Blocklisting
{
    public interface IBlocklistService
    {
        bool Blocklisted(int gameId, ReleaseInfo release);
        bool BlocklistedTorrentHash(int gameId, string hash);
        PagingSpec<Blocklist> Paged(PagingSpec<Blocklist> pagingSpec);
        void Block(RemoteEpisode remoteRom, string message, string source);
        void Delete(int id);
        void Delete(List<int> ids);
    }

    public class BlocklistService : IBlocklistService,
                                    IExecute<ClearBlocklistCommand>,
                                    IHandle<DownloadFailedEvent>,
                                    IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IBlocklistRepository _blocklistRepository;

        public BlocklistService(IBlocklistRepository blocklistRepository)
        {
            _blocklistRepository = blocklistRepository;
        }

        public bool Blocklisted(int gameId, ReleaseInfo release)
        {
            if (release.DownloadProtocol == DownloadProtocol.Torrent)
            {
                if (release is not TorrentInfo torrentInfo)
                {
                    return false;
                }

                if (torrentInfo.InfoHash.IsNotNullOrWhiteSpace())
                {
                    var blocklistedByTorrentInfohash = _blocklistRepository.BlocklistedByTorrentInfoHash(gameId, torrentInfo.InfoHash);

                    return blocklistedByTorrentInfohash.Any(b => SameTorrent(b, torrentInfo));
                }

                return _blocklistRepository.BlocklistedByTitle(gameId, release.Title)
                    .Where(b => b.Protocol == DownloadProtocol.Torrent)
                    .Any(b => SameTorrent(b, torrentInfo));
            }

            return _blocklistRepository.BlocklistedByTitle(gameId, release.Title)
                .Where(b => b.Protocol == DownloadProtocol.Usenet)
                .Any(b => SameNzb(b, release));
        }

        public bool BlocklistedTorrentHash(int gameId, string hash)
        {
            return _blocklistRepository.BlocklistedByTorrentInfoHash(gameId, hash).Any(b =>
                b.TorrentInfoHash.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
        }

        public PagingSpec<Blocklist> Paged(PagingSpec<Blocklist> pagingSpec)
        {
            return _blocklistRepository.GetPaged(pagingSpec);
        }

        public void Block(RemoteEpisode remoteRom, string message, string source)
        {
            var blocklist = new Blocklist
                            {
                                SeriesId = remoteRom.Game.Id,
                                RomIds = remoteRom.Roms.Select(e => e.Id).ToList(),
                                SourceTitle =  remoteRom.Release.Title,
                                Quality = remoteRom.ParsedRomInfo.Quality,
                                Date = DateTime.UtcNow,
                                PublishedDate = remoteRom.Release.PublishDate,
                                Size = remoteRom.Release.Size,
                                Indexer = remoteRom.Release.Indexer,
                                Protocol = remoteRom.Release.DownloadProtocol,
                                Message = message,
                                Source = source,
                                Languages = remoteRom.ParsedRomInfo.Languages
                            };

            if (remoteRom.Release is TorrentInfo torrentRelease)
            {
                blocklist.TorrentInfoHash = torrentRelease.InfoHash;
            }

            _blocklistRepository.Insert(blocklist);
        }

        public void Delete(int id)
        {
            _blocklistRepository.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _blocklistRepository.DeleteMany(ids);
        }

        private bool SameNzb(Blocklist item, ReleaseInfo release)
        {
            return ReleaseComparer.SameNzb(new ReleaseComparerModel(item), release);
        }

        private bool SameTorrent(Blocklist item, TorrentInfo release)
        {
            return ReleaseComparer.SameTorrent(new ReleaseComparerModel(item), release);
        }

        public void Execute(ClearBlocklistCommand message)
        {
            _blocklistRepository.Purge();
        }

        public void Handle(DownloadFailedEvent message)
        {
            var blocklist = new Blocklist
            {
                SeriesId = message.SeriesId,
                RomIds = message.RomIds,
                SourceTitle = message.SourceTitle,
                Quality = message.Quality,
                Date = DateTime.UtcNow,
                PublishedDate = DateTime.Parse(message.Data.GetValueOrDefault("publishedDate")),
                Size = long.Parse(message.Data.GetValueOrDefault("size", "0")),
                Indexer = message.Data.GetValueOrDefault("indexer"),
                Protocol = (DownloadProtocol)Convert.ToInt32(message.Data.GetValueOrDefault("protocol")),
                Message = message.Message,
                Source = message.Source,
                Languages = message.Languages,
                TorrentInfoHash = message.TrackedDownload?.Protocol == DownloadProtocol.Torrent
                    ? message.TrackedDownload.DownloadItem.DownloadId
                    : message.Data.GetValueOrDefault("torrentInfoHash", null)
            };

            if (Enum.TryParse(message.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
            {
                blocklist.IndexerFlags = flags;
            }

            if (Enum.TryParse(message.Data.GetValueOrDefault("releaseType"), true, out ReleaseType releaseType))
            {
                blocklist.ReleaseType = releaseType;
            }

            _blocklistRepository.Insert(blocklist);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            _blocklistRepository.DeleteForGameIds(message.Game.Select(m => m.Id).ToList());
        }
    }
}
