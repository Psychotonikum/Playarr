using Playarr.Core.Annotations;

namespace Playarr.Core.Notifications
{
    public enum MetadataLinkType
    {
        [FieldOption(Label = "IMDb")]
        Imdb = 0,

        [FieldOption(Label = "TVDb")]
        Igdb = 1,

        [FieldOption(Label = "TVMaze")]
        Tvmaze = 2,

        [FieldOption(Label = "Trakt")]
        Trakt = 3
    }
}
