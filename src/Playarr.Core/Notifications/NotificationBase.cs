using System;
using System.Collections.Generic;
using FluentValidation.Results;
using Playarr.Core.MediaFiles;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        protected const string EPISODE_GRABBED_TITLE = "Rom Grabbed";
        protected const string EPISODE_DOWNLOADED_TITLE = "Rom Downloaded";
        protected const string IMPORT_COMPLETE_TITLE = "Import Complete";
        protected const string EPISODE_DELETED_TITLE = "Rom Deleted";
        protected const string SERIES_ADDED_TITLE = "Game Added";
        protected const string SERIES_DELETED_TITLE = "Game Deleted";
        protected const string HEALTH_ISSUE_TITLE = "Health Check Failure";
        protected const string HEALTH_RESTORED_TITLE = "Health Check Restored";
        protected const string APPLICATION_UPDATE_TITLE = "Application Updated";
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE = "Manual Interaction";

        protected const string EPISODE_GRABBED_TITLE_BRANDED = "Playarr - " + EPISODE_GRABBED_TITLE;
        protected const string EPISODE_DOWNLOADED_TITLE_BRANDED = "Playarr - " + EPISODE_DOWNLOADED_TITLE;
        protected const string IMPORT_COMPLETE_TITLE_BRANDED = "Playarr - " + IMPORT_COMPLETE_TITLE;
        protected const string EPISODE_DELETED_TITLE_BRANDED = "Playarr - " + EPISODE_DELETED_TITLE;
        protected const string SERIES_ADDED_TITLE_BRANDED = "Playarr - " + SERIES_ADDED_TITLE;
        protected const string SERIES_DELETED_TITLE_BRANDED = "Playarr - " + SERIES_DELETED_TITLE;
        protected const string HEALTH_ISSUE_TITLE_BRANDED = "Playarr - " + HEALTH_ISSUE_TITLE;
        protected const string HEALTH_RESTORED_TITLE_BRANDED = "Playarr - " + HEALTH_RESTORED_TITLE;
        protected const string APPLICATION_UPDATE_TITLE_BRANDED = "Playarr - " + APPLICATION_UPDATE_TITLE;
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED = "Playarr - " + MANUAL_INTERACTION_REQUIRED_TITLE;

        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();

        public abstract string Link { get; }

        public virtual void OnGrab(GrabMessage grabMessage)
        {
        }

        public virtual void OnDownload(DownloadMessage message)
        {
        }

        public virtual void OnImportComplete(ImportCompleteMessage message)
        {
        }

        public virtual void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
        }

        public virtual void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
        }

        public virtual void OnSeriesAdd(SeriesAddMessage message)
        {
        }

        public virtual void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
        }

        public virtual void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
        }

        public virtual void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
        }

        public virtual void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
        }

        public virtual void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
        }

        public virtual void ProcessQueue()
        {
        }

        public bool SupportsOnGrab => HasConcreteImplementation("OnGrab");
        public bool SupportsOnRename => HasConcreteImplementation("OnRename");
        public bool SupportsOnDownload => HasConcreteImplementation("OnDownload");
        public bool SupportsOnUpgrade => SupportsOnDownload;
        public bool SupportsOnImportComplete => HasConcreteImplementation("OnImportComplete");
        public bool SupportsOnSeriesAdd => HasConcreteImplementation("OnSeriesAdd");
        public bool SupportsOnSeriesDelete => HasConcreteImplementation("OnSeriesDelete");
        public bool SupportsOnRomFileDelete => HasConcreteImplementation("OnRomFileDelete");
        public bool SupportsOnRomFileDeleteForUpgrade => SupportsOnRomFileDelete;
        public bool SupportsOnHealthIssue => HasConcreteImplementation("OnHealthIssue");
        public bool SupportsOnHealthRestored => HasConcreteImplementation("OnHealthRestored");
        public bool SupportsOnApplicationUpdate => HasConcreteImplementation("OnApplicationUpdate");
        public bool SupportsOnManualInteractionRequired => HasConcreteImplementation("OnManualInteractionRequired");

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        private bool HasConcreteImplementation(string methodName)
        {
            var method = GetType().GetMethod(methodName);

            if (method == null)
            {
                throw new MissingMethodException(GetType().Name, Name);
            }

            return !method.DeclaringType.IsAbstract;
        }
    }
}
