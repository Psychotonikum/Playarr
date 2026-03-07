using System;
using System.Collections.Generic;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookRomFile
    {
        public WebhookRomFile()
        {
        }

        public WebhookRomFile(RomFile romFile)
        {
            Id = romFile.Id;
            RelativePath = romFile.RelativePath;
            Path = System.IO.Path.Combine(romFile.Game.Value.Path, romFile.RelativePath);
            Quality = romFile.Quality.Quality.Name;
            QualityVersion = romFile.Quality.Revision.Version;
            ReleaseGroup = romFile.ReleaseGroup;
            SceneName = romFile.SceneName;
            Size = romFile.Size;
            DateAdded = romFile.DateAdded;
            Languages = romFile.Languages;

            if (romFile.MediaInfo != null)
            {
                MediaInfo = new WebhookRomFileMediaInfo(romFile);
            }
        }

        public int Id { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public List<Language> Languages { get; set; }
        public WebhookRomFileMediaInfo MediaInfo { get; set; }
        public string SourcePath { get; set; }
        public string RecycleBinPath { get; set; }
    }
}
