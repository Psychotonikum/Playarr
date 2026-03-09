using Playarr.Core.Annotations;

namespace Playarr.Core.Games
{
    public enum RomModificationType
    {
        [FieldOption(label: "Original")]
        Original = 0,

        [FieldOption(label: "Hack")]
        Hack = 1,

        [FieldOption(label: "Translation")]
        Translation = 2,

        [FieldOption(label: "Homebrew")]
        Homebrew = 3,

        [FieldOption(label: "Unlicensed")]
        Unlicensed = 4,

        [FieldOption(label: "Pirate")]
        Pirate = 5
    }
}
