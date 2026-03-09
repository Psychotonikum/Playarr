using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Datastore;

namespace Playarr.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QualitySource Source { get; set; }
        public int Resolution { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, QualitySource source, int resolution)
        {
            Id = id;
            Name = name;
            Source = source;
            Resolution = resolution;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        // ROM Quality levels — ordered by verification confidence
        public static Quality Unknown => new Quality(0, "Unknown", QualitySource.Unknown, 0);
        public static Quality Bad => new Quality(1, "Bad", QualitySource.CRC, 0);
        public static Quality Verified => new Quality(2, "Verified", QualitySource.CRC, 0);

        // Keep legacy aliases that map to new values (for backward compat in tests/parsers)
        public static Quality SDTV => Unknown;
        public static Quality DVD => Unknown;
        public static Quality WEBDL1080p => Unknown;
        public static Quality HDTV720p => Unknown;
        public static Quality WEBDL720p => Unknown;
        public static Quality Bluray720p => Unknown;
        public static Quality Bluray1080p => Unknown;
        public static Quality WEBDL480p => Unknown;
        public static Quality HDTV1080p => Unknown;
        public static Quality RAWHD => Unknown;
        public static Quality WEBRip480p => Unknown;
        public static Quality Bluray480p => Unknown;
        public static Quality Bluray576p => Unknown;
        public static Quality WEBRip720p => Unknown;
        public static Quality WEBRip1080p => Unknown;
        public static Quality HDTV2160p => Unknown;
        public static Quality WEBRip2160p => Unknown;
        public static Quality WEBDL2160p => Unknown;
        public static Quality Bluray2160p => Unknown;
        public static Quality Bluray1080pRemux => Unknown;
        public static Quality Bluray2160pRemux => Unknown;

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                Bad,
                Verified
            };

            AllLookup = All.ToDictionary(q => q.Id, q => q);

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)  { Weight = 1, MinSize = 0, MaxSize = null, PreferredSize = 95 },
                new QualityDefinition(Quality.Bad)      { Weight = 2, MinSize = 0, MaxSize = null, PreferredSize = 95 },
                new QualityDefinition(Quality.Verified)  { Weight = 3, MinSize = 0, MaxSize = null, PreferredSize = 95 }
            };
        }

        public static readonly List<Quality> All;

        public static readonly Dictionary<int, Quality> AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;

        public static Quality FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            if (!AllLookup.TryGetValue(id, out var quality))
            {
                // Legacy IDs from the TV-oriented quality system all map to Unknown
                return Unknown;
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
