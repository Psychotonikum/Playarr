using Playarr.Core.MediaFiles;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookRenamedRomFile : WebhookRomFile
    {
        public WebhookRenamedRomFile(RenamedRomFile renamedEpisode)
            : base(renamedEpisode.RomFile)
        {
            PreviousRelativePath = renamedEpisode.PreviousRelativePath;
            PreviousPath = renamedEpisode.PreviousPath;
        }

        public string PreviousRelativePath { get; set; }
        public string PreviousPath { get; set; }
    }
}
