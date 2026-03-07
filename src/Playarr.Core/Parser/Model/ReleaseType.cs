using Playarr.Core.Annotations;

namespace Playarr.Core.Parser.Model
{
    public enum ReleaseType
    {
        Unknown = 0,

        [FieldOption(label: "Single Rom")]
        SingleEpisode = 1,

        [FieldOption(label: "Multi-Rom")]
        MultiEpisode = 2,

        [FieldOption(label: "Platform Pack")]
        SeasonPack = 3
    }
}
