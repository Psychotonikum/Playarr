using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Core.Localization;
using Playarr.Core.MediaCover;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Gotify
{
    public class Gotify : NotificationBase<GotifySettings>
    {
        private const string PlayarrImageUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/128.png";

        private readonly IGotifyProxy _proxy;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public Gotify(IGotifyProxy proxy, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Name => "Gotify";
        public override string Link => "https://gotify.net/";

        public override void OnGrab(GrabMessage message)
        {
            SendNotification(EPISODE_GRABBED_TITLE, message.Message, message.Game);
        }

        public override void OnDownload(DownloadMessage message)
        {
            SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, message.Game);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            SendNotification(IMPORT_COMPLETE_TITLE, message.Message, message.Game);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage message)
        {
            SendNotification(EPISODE_DELETED_TITLE, message.Message, message.Game);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            SendNotification(SERIES_ADDED_TITLE, message.Message, message.Game);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage message)
        {
            SendNotification(SERIES_DELETED_TITLE, message.Message, message.Game);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, null);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            SendNotification(HEALTH_RESTORED_TITLE, $"The following issue is now resolved: {previousCheck.Message}", null);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage message)
        {
            SendNotification(APPLICATION_UPDATE_TITLE, message.Message, null);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE, message.Message, message.Game);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                var isMarkdown = false;
                const string title = "Test Notification";

                var sb = new StringBuilder();
                sb.AppendLine("This is a test message from Playarr");

                var payload = new GotifyMessage
                {
                    Title = title,
                    Priority = Settings.Priority
                };

                if (Settings.IncludeGamePoster)
                {
                    isMarkdown = true;

                    sb.AppendLine($"\r![]({PlayarrImageUrl})");
                    payload.SetImage(PlayarrImageUrl);
                }

                if (Settings.MetadataLinks.Any())
                {
                    isMarkdown = true;

                    sb.AppendLine("");
                    sb.AppendLine("[Playarr.tv](https://playarr.tv)");
                    payload.SetClickUrl("https://playarr.tv");
                }

                payload.Message = sb.ToString();
                payload.SetContentType(isMarkdown);

                _proxy.SendNotification(payload, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                failures.Add(new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
            }

            return new ValidationResult(failures);
        }

        private void SendNotification(string title, string message, Game game)
        {
            var isMarkdown = false;
            var sb = new StringBuilder();

            sb.AppendLine(message);

            var payload = new GotifyMessage
            {
                Title = title,
                Priority = Settings.Priority
            };

            if (game != null)
            {
                if (Settings.IncludeGamePoster)
                {
                    var poster = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;

                    if (poster != null)
                    {
                        isMarkdown = true;
                        sb.AppendLine($"\r![]({poster})");
                        payload.SetImage(poster);
                    }
                }

                if (Settings.MetadataLinks.Any())
                {
                    isMarkdown = true;
                    sb.AppendLine("");

                    foreach (var link in Settings.MetadataLinks)
                    {
                        var linkType = (MetadataLinkType)link;
                        var linkText = "";
                        var linkUrl = "";

                        if (linkType == MetadataLinkType.Imdb && game.ImdbId.IsNotNullOrWhiteSpace())
                        {
                            linkText = "IMDb";
                            linkUrl = $"https://www.imdb.com/title/{game.ImdbId}";
                        }

                        if (linkType == MetadataLinkType.Igdb && game.IgdbId > 0)
                        {
                            linkText = "TVDb";
                            linkUrl = $"http://www.theigdb.com/?tab=game&id={game.IgdbId}";
                        }

                        if (linkType == MetadataLinkType.Trakt && game.IgdbId > 0)
                        {
                            linkText = "Trakt";
                            linkUrl = $"http://trakt.tv/search/igdb/{game.IgdbId}?id_type=show";
                        }

                        if (linkType == MetadataLinkType.Tvmaze && game.RawgId > 0)
                        {
                            linkText = "TVMaze";
                            linkUrl = $"http://www.tvmaze.com/shows/{game.RawgId}/_";
                        }

                        sb.AppendLine($"[{linkText}]({linkUrl})");

                        if (link == Settings.PreferredMetadataLink)
                        {
                            payload.SetClickUrl(linkUrl);
                        }
                    }
                }
            }

            payload.Message = sb.ToString();
            payload.SetContentType(isMarkdown);

            _proxy.SendNotification(payload, Settings);
        }
    }
}
