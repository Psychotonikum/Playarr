using System;
using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.MediaFiles;

namespace Playarr.Core.Games
{
    public class Rom : ModelBase, IComparable
    {
        public Rom()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public const string AIR_DATE_FORMAT = "yyyy-MM-dd";

        public int GameId { get; set; }
        public int IgdbId { get; set; }
        public int EpisodeFileId { get; set; }
        public int PlatformNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }
        public int? ScenePlatformNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? AiredAfterPlatformNumber { get; set; }
        public int? AiredBeforePlatformNumber { get; set; }
        public int? AiredBeforeRomNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public Ratings Ratings { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }

        public string GameTitle { get; private set; }

        public LazyLoaded<RomFile> RomFile { get; set; }

        public Game Game { get; set; }

        public bool HasFile => EpisodeFileId > 0;
        public bool AbsoluteRomNumberAdded { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", Id, Title.NullSafe());
        }

        public int CompareTo(object obj)
        {
            var other = (Rom)obj;

            if (PlatformNumber > other.PlatformNumber)
            {
                return 1;
            }

            if (PlatformNumber < other.PlatformNumber)
            {
                return -1;
            }

            if (EpisodeNumber > other.EpisodeNumber)
            {
                return 1;
            }

            if (EpisodeNumber < other.EpisodeNumber)
            {
                return -1;
            }

            return 0;
        }
    }
}
