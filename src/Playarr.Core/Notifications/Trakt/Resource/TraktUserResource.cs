using Playarr.Core.Notifications.Trakt.Resource;

namespace Playarr.Core.Notifications.Trakt
{
    public class TraktUserResource
    {
        public string Username { get; set; }
        public TraktUserIdsResource Ids { get; set; }
    }
}
