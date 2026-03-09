using Playarr.Core.Annotations;

namespace Playarr.Core.Games
{
    public enum RomRegion
    {
        [FieldOption(label: "Unknown")]
        Unknown = 0,

        [FieldOption(label: "USA")]
        USA = 1,

        [FieldOption(label: "Europe")]
        Europe = 2,

        [FieldOption(label: "Japan")]
        Japan = 3,

        [FieldOption(label: "World")]
        World = 4,

        [FieldOption(label: "Asia")]
        Asia = 5,

        [FieldOption(label: "Australia")]
        Australia = 6,

        [FieldOption(label: "Korea")]
        Korea = 7,

        [FieldOption(label: "Brazil")]
        Brazil = 8,

        [FieldOption(label: "Other")]
        Other = 99
    }
}
