using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Core.CustomFormats;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Download.Clients;
using Playarr.Core.Languages;
using Playarr.Core.Games;

namespace Playarr.Core.Parser.Model
{
    public class RemoteRom
    {
        public ReleaseInfo Release { get; set; }
        public ParsedRomInfo ParsedRomInfo { get; set; }
        public SceneMapping SceneMapping { get; set; }
        public int MappedPlatformNumber { get; set; }
        public Game Game { get; set; }
        public List<Rom> Roms { get; set; }
        public bool EpisodeRequested { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public SeriesMatchType SeriesMatchType { get; set; }
        public List<Language> Languages { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }

        public RemoteRom()
        {
            Roms = new List<Rom>();
            CustomFormats = new List<CustomFormat>();
            Languages = new List<Language>();
        }

        public bool IsRecentEpisode()
        {
            return Roms.Any(e => e.AirDateUtc >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }

    public enum ReleaseSourceType
    {
        Unknown = 0,
        Rss = 1,
        Search = 2,
        UserInvokedSearch = 3,
        InteractiveSearch = 4,
        ReleasePush = 5
    }
}
