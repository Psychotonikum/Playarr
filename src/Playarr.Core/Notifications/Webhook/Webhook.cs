using System.Collections.Generic;
using FluentValidation.Results;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.Tags;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.Webhook
{
    public class Webhook : WebhookBase<WebhookSettings>
    {
        private readonly IWebhookProxy _proxy;

        public Webhook(IWebhookProxy proxy, IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository, IMapCoversToLocal mediaCoverService)
            : base(configFileProvider, configService, localizationService, tagRepository, mediaCoverService)
        {
            _proxy = proxy;
        }

        public override string Link => "https://wiki.servarr.com/playarr/settings#connections";

        public override void OnGrab(GrabMessage message)
        {
            _proxy.SendWebhook(BuildOnGrabPayload(message), Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendWebhook(BuildOnDownloadPayload(message), Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            _proxy.SendWebhook(BuildOnImportCompletePayload(message), Settings);
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            _proxy.SendWebhook(BuildOnRenamePayload(game, renamedFiles), Settings);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendWebhook(BuildOnRomFileDelete(deleteMessage), Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            _proxy.SendWebhook(BuildOnSeriesAdd(message), Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _proxy.SendWebhook(BuildOnSeriesDelete(deleteMessage), Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendWebhook(BuildHealthPayload(healthCheck), Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendWebhook(BuildHealthRestoredPayload(previousCheck), Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendWebhook(BuildApplicationUpdatePayload(updateMessage), Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            _proxy.SendWebhook(BuildManualInteractionRequiredPayload(message), Settings);
        }

        public override string Name => "Webhook";

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
                _proxy.SendWebhook(BuildTestPayload(), Settings);
            }
            catch (WebhookException ex)
            {
                return new PlayarrValidationFailure("Url", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
