using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;

namespace Playarr.Core.Parser.Model
{
    public class ParsedRomInfo
    {
        public string ReleaseTitle { get; set; }
        public string GameTitle { get; set; }
        public GameTitleInfo GameTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int PlatformNumber { get; set; }
        public int[] RomNumbers { get; set; }
        public int[] AbsoluteRomNumbers { get; set; }
        public decimal[] SpecialAbsoluteRomNumbers { get; set; }
        public string AirDate { get; set; }
        public List<Language> Languages { get; set; }
        public bool FullSeason { get; set; }
        public bool IsPartialSeason { get; set; }
        public bool IsMultiSeason { get; set; }
        public bool IsSeasonExtra { get; set; }
        public bool IsSplitEpisode { get; set; }
        public bool IsMiniSeries { get; set; }
        public bool Special { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public int SeasonPart { get; set; }
        public string ReleaseTokens { get; set; }
        public int? DailyPart { get; set; }

        public ParsedRomInfo()
        {
            RomNumbers = Array.Empty<int>();
            AbsoluteRomNumbers = Array.Empty<int>();
            SpecialAbsoluteRomNumbers = Array.Empty<decimal>();
            Languages = new List<Language>();
        }

        public bool IsDaily
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AirDate);
            }

            private set
            {
            }
        }

        public bool IsAbsoluteNumbering
        {
            get
            {
                return AbsoluteRomNumbers.Any();
            }

            private set
            {
            }
        }

        public bool IsPossibleSpecialEpisode
        {
            get
            {
                return ((AirDate.IsNullOrWhiteSpace() &&
                       GameTitle.IsNullOrWhiteSpace() &&
                       (RomNumbers.Length == 0 || PlatformNumber == 0)) || (!GameTitle.IsNullOrWhiteSpace() && Special)) ||
                       (RomNumbers.Length == 1 && RomNumbers[0] == 0);
            }

            private set
            {
            }
        }

        public bool IsPossibleSceneSeasonSpecial
        {
            get
            {
                return PlatformNumber != 0 && RomNumbers.Length == 1 && RomNumbers[0] == 0;
            }

            private set
            {
            }
        }

        public ReleaseType ReleaseType
        {
            get
            {
                if (RomNumbers.Length > 1 || AbsoluteRomNumbers.Length > 1)
                {
                    return Model.ReleaseType.MultiEpisode;
                }

                if (RomNumbers.Length == 1 || AbsoluteRomNumbers.Length == 1)
                {
                    return Model.ReleaseType.SingleEpisode;
                }

                if (FullSeason)
                {
                    return Model.ReleaseType.SeasonPack;
                }

                return Model.ReleaseType.Unknown;
            }
        }

        public override string ToString()
        {
            var episodeString = "[Unknown Rom]";

            if (IsDaily && RomNumbers.Empty())
            {
                episodeString = string.Format("{0}", AirDate);
            }
            else if (FullSeason)
            {
                episodeString = string.Format("Platform {0:00}", PlatformNumber);
            }
            else if (RomNumbers != null && RomNumbers.Any())
            {
                episodeString = string.Format("S{0:00}E{1}", PlatformNumber, string.Join("-", RomNumbers.Select(c => c.ToString("00"))));
            }
            else if (AbsoluteRomNumbers != null && AbsoluteRomNumbers.Any())
            {
                episodeString = string.Format("{0}", string.Join("-", AbsoluteRomNumbers.Select(c => c.ToString("000"))));
            }
            else if (Special)
            {
                if (PlatformNumber != 0)
                {
                    episodeString = string.Format("[Unknown Platform {0:00} Special]", PlatformNumber);
                }
                else
                {
                    episodeString = "[Unknown Special]";
                }
            }

            return string.Format("{0} - {1} {2}", GameTitle, episodeString, Quality);
        }
    }
}
