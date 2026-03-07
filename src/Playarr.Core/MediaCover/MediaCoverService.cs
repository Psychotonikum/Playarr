using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Common.Http;
using Playarr.Core.Configuration;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games;
using Playarr.Core.Games.Events;

namespace Playarr.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int gameId, IEnumerable<MediaCover> covers);
        string GetCoverPath(int gameId, MediaCoverTypes coverType, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<SeriesUpdatedEvent>,
        IHandleAsync<SeriesDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IMediaCoverProxy _mediaCoverProxy;
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        // ImageSharp is slow on ARM (no hardware acceleration on mono yet)
        // So limit the number of concurrent resizing tasks
        private static SemaphoreSlim _semaphore = new SemaphoreSlim((int)Math.Ceiling(Environment.ProcessorCount / 2.0));

        public MediaCoverService(IMediaCoverProxy mediaCoverProxy,
                                 IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _mediaCoverProxy = mediaCoverProxy;
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int gameId, MediaCoverTypes coverType, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetSeriesCoverPath(gameId), coverType.ToString().ToLower() + heightSuffix + GetExtension(coverType));
        }

        public void ConvertToLocalUrls(int gameId, IEnumerable<MediaCover> covers)
        {
            if (gameId == 0)
            {
                // Game isn't in Playarr yet, map via a proxy to circumvent referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    if (mediaCover.CoverType == MediaCoverTypes.Unknown)
                    {
                        continue;
                    }

                    var filePath = GetCoverPath(gameId, mediaCover.CoverType);

                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + gameId + "/" + mediaCover.CoverType.ToString().ToLower() + GetExtension(mediaCover.CoverType);

                    if (_diskProvider.FileExists(filePath))
                    {
                        var lastWrite = _diskProvider.FileGetLastWrite(filePath);
                        mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                    }
                }
            }
        }

        private string GetSeriesCoverPath(int gameId)
        {
            return Path.Combine(_coverRootFolder, gameId.ToString());
        }

        private bool EnsureCovers(Game game)
        {
            var updated = false;
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in game.Images)
            {
                if (cover.CoverType == MediaCoverTypes.Unknown)
                {
                    continue;
                }

                var fileName = GetCoverPath(game.Id, cover.CoverType);
                var alreadyExists = false;

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.RemoteUrl, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(game, cover);
                        updated = true;
                    }
                }
                catch (HttpException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", game, e.Message);
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", game, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", game);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(game, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return updated;
        }

        private void DownloadCover(Game game, MediaCover cover)
        {
            var fileName = GetCoverPath(game.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, game, cover.RemoteUrl);
            _httpClient.DownloadFile(cover.RemoteUrl, fileName);
        }

        private void EnsureResizedCovers(Game game, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetCoverPath(game.Id, cover.CoverType);
                var resizeFileName = GetCoverPath(game.Id, cover.CoverType, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, game);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, game);
                    }
                }
            }
        }

        private string GetExtension(MediaCoverTypes coverType)
        {
            switch (coverType)
            {
                default:
                    return ".jpg";

                case MediaCoverTypes.Clearlogo:
                    return ".png";
            }
        }

        public void HandleAsync(SeriesUpdatedEvent message)
        {
            var updated = EnsureCovers(message.Game);

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Game, updated));
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            foreach (var game in message.Game)
            {
                var path = GetSeriesCoverPath(game.Id);
                if (_diskProvider.FolderExists(path))
                {
                    _diskProvider.DeleteFolder(path, true);
                }
            }
        }
    }
}
