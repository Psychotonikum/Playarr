using System.Collections.Generic;
using FluentValidation.Results;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private readonly ITelegramProxy _proxy;
        private readonly IConfigFileProvider _configFileProvider;

        public Telegram(ITelegramProxy proxy, IConfigFileProvider configFileProvider)
        {
            _proxy = proxy;
            _configFileProvider = configFileProvider;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        private string InstanceName => _configFileProvider.InstanceName;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_GRABBED_TITLE_BRANDED : EPISODE_GRABBED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(grabMessage.Game);

            _proxy.SendNotification(title, grabMessage.Message, links, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DOWNLOADED_TITLE_BRANDED : EPISODE_DOWNLOADED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(message.Game);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DOWNLOADED_TITLE_BRANDED : EPISODE_DOWNLOADED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(message.Game);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DELETED_TITLE_BRANDED : EPISODE_DELETED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(deleteMessage.Game);

            _proxy.SendNotification(title, deleteMessage.Message, links, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? SERIES_ADDED_TITLE_BRANDED : SERIES_ADDED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(message.Game);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? SERIES_DELETED_TITLE_BRANDED : SERIES_DELETED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(deleteMessage.Game);

            _proxy.SendNotification(title, deleteMessage.Message, links, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_ISSUE_TITLE_BRANDED : HEALTH_ISSUE_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, healthCheck.Message, new List<NotificationMetadataLink>(), Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_RESTORED_TITLE_BRANDED : HEALTH_RESTORED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, $"The following issue is now resolved: {previousCheck.Message}", new List<NotificationMetadataLink>(), Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? APPLICATION_UPDATE_TITLE_BRANDED : APPLICATION_UPDATE_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, updateMessage.Message, new List<NotificationMetadataLink>(), Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED : MANUAL_INTERACTION_REQUIRED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;
            var links = GetLinks(message.Game);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        private List<NotificationMetadataLink> GetLinks(Game game)
        {
            return NotificationMetadataLinkGenerator.GenerateLinks(game, Settings.MetadataLinks);
        }
    }
}
