using System.Collections.Generic;
using Playarr.Core.MediaFiles;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnDownload(DownloadMessage message);
        void OnRename(Game game, List<RenamedRomFile> renamedFiles);
        void OnImportComplete(ImportCompleteMessage message);
        void OnRomFileDelete(EpisodeDeleteMessage deleteMessage);
        void OnSeriesAdd(SeriesAddMessage message);
        void OnSeriesDelete(SeriesDeleteMessage deleteMessage);
        void OnHealthIssue(HealthCheck.HealthCheck healthCheck);
        void OnHealthRestored(HealthCheck.HealthCheck previousCheck);
        void OnApplicationUpdate(ApplicationUpdateMessage updateMessage);
        void OnManualInteractionRequired(ManualInteractionRequiredMessage message);
        void ProcessQueue();
        bool SupportsOnGrab { get; }
        bool SupportsOnDownload { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnImportComplete { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnSeriesAdd { get; }
        bool SupportsOnSeriesDelete { get; }
        bool SupportsOnRomFileDelete { get; }
        bool SupportsOnRomFileDeleteForUpgrade { get; }
        bool SupportsOnHealthIssue { get; }
        bool SupportsOnHealthRestored { get; }
        bool SupportsOnApplicationUpdate { get; }
        bool SupportsOnManualInteractionRequired { get; }
    }
}
