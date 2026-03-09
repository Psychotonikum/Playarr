using Playarr.Core.Annotations;

namespace Playarr.Core.Games
{
    public enum DumpQuality
    {
        [FieldOption(label: "Unknown")]
        Unknown = 0,

        [FieldOption(label: "Verified")]
        Verified = 1,

        [FieldOption(label: "Bad")]
        Bad = 2,

        [FieldOption(label: "Overdump")]
        Overdump = 3,

        [FieldOption(label: "Underdump")]
        Underdump = 4,

        [FieldOption(label: "Modified")]
        Modified = 5
    }
}
