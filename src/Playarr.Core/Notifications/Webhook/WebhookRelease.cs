using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookRelease
    {
        public WebhookRelease()
        {
        }

        public WebhookRelease(QualityModel quality, RemoteEpisode remoteRom)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            ReleaseGroup = remoteRom.ParsedRomInfo.ReleaseGroup;
            ReleaseTitle = remoteRom.Release.Title;
            Indexer = remoteRom.Release.Indexer;
            Size = remoteRom.Release.Size;
            CustomFormats = remoteRom.CustomFormats?.Select(x => x.Name).ToList();
            CustomFormatScore = remoteRom.CustomFormatScore;
            Languages = remoteRom.Languages;
            IndexerFlags = Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Where(r => (remoteRom.Release.IndexerFlags & r) == r).Select(r => r.ToString()).ToList();
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
        public int CustomFormatScore { get; set; }
        public List<string> CustomFormats { get; set; }
        public List<Language> Languages { get; set; }
        public List<string> IndexerFlags { get; set; }
    }
}
