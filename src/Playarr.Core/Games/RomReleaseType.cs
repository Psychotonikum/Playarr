using Playarr.Core.Annotations;

namespace Playarr.Core.Games
{
    public enum RomReleaseType
    {
        [FieldOption(label: "Retail")]
        Retail = 0,

        [FieldOption(label: "Prototype")]
        Prototype = 1,

        [FieldOption(label: "Beta")]
        Beta = 2,

        [FieldOption(label: "Demo")]
        Demo = 3,

        [FieldOption(label: "Sample")]
        Sample = 4,

        [FieldOption(label: "Promo")]
        Promo = 5,

        [FieldOption(label: "Competition")]
        Competition = 6,

        [FieldOption(label: "Update")]
        Update = 7,

        [FieldOption(label: "DLC")]
        Dlc = 8
    }
}
