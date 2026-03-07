using Playarr.Core.Annotations;

namespace Playarr.Core.ImportLists.Trakt.User
{
    public enum TraktUserListType
    {
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeWatch")]
        UserWatchList = 0,
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeWatched")]
        UserWatchedList = 1,
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeCollection")]
        UserCollectionList = 2
    }
}
