using System.Collections.Generic;
using FluentValidation.Results;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.Notifications.Webhook;
using Playarr.Core.Tags;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.Notifiarr
{
    public class Notifiarr : WebhookBase<NotifiarrSettings>
    {
        private readonly INotifiarrProxy _proxy;

        public Notifiarr(INotifiarrProxy proxy, IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository, IMapCoversToLocal mediaCoverService)
            : base(configFileProvider, configService, localizationService, tagRepository, mediaCoverService)
        {
            _proxy = proxy;
        }

        public override string Link => "https://notifiarr.com";
        public override string Name => "Notifiarr";

        public override void OnGrab(GrabMessage message)
        {
            _proxy.SendNotification(BuildOnGrabPayload(message), Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(BuildOnDownloadPayload(message), Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            _proxy.SendNotification(BuildOnImportCompletePayload(message), Settings);
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            _proxy.SendNotification(BuildOnRenamePayload(game, renamedFiles), Settings);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(BuildOnRomFileDelete(deleteMessage), Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            _proxy.SendNotification(BuildOnSeriesAdd(message), Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(BuildOnSeriesDelete(deleteMessage), Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(BuildHealthPayload(healthCheck), Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendNotification(BuildHealthRestoredPayload(previousCheck), Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(BuildApplicationUpdatePayload(updateMessage), Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            _proxy.SendNotification(BuildManualInteractionRequiredPayload(message), Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(SendWebhookTest());

            return new ValidationResult(failures);
        }

        private ValidationFailure SendWebhookTest()
        {
            try
            {
                _proxy.SendNotification(BuildTestPayload(), Settings);
            }
            catch (NotifiarrException ex)
            {
                return new PlayarrValidationFailure("APIKey", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
