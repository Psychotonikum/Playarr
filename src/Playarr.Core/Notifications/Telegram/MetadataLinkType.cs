using Playarr.Core.Annotations;

namespace Playarr.Core.Notifications.Telegram
{
    // Maintain the same values as MetadataLinkType

    public enum MetadataLinkPreviewType
    {
        [FieldOption(Label = "None")]
        None = -1,

        [FieldOption(Label = "IMDb")]
        Imdb = 0,

        // No preview data is supported for TheIGDB at this time
        // [FieldOption(Label = "TVDb")]
        // Igdb = 1,

        [FieldOption(Label = "TVMaze")]
        Tvmaze = 2,

        [FieldOption(Label = "Trakt")]
        Trakt = 3
    }
}
