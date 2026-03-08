using System.Collections.Generic;
using Newtonsoft.Json;

namespace Playarr.Core.ImportLists.Trakt
{
    public class TraktGameIdsResource
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int? Tmdb { get; set; }
        public int? Igdb { get; set; }
    }

    public class TraktGameResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktGameIdsResource Ids { get; set; }
        [JsonProperty("aired_episodes")]
        public int AiredEpisodes { get; set; }
    }

    public class TraktResponse
    {
        public TraktGameResource Show { get; set; }
    }

    public class TraktWatchedRomResource
    {
        public int? Plays { get; set; }
    }

    public class TraktWatchedSeasonResource
    {
        public int? Number { get; set; }
        public List<TraktWatchedRomResource> Roms { get; set; }
    }

    public class TraktWatchedResponse : TraktResponse
    {
        public List<TraktWatchedSeasonResource> Platforms { get; set; }
    }

    public class RefreshRequestResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class UserSettingsResponse
    {
        public TraktUserResource User { get; set; }
    }

    public class TraktUserResource
    {
        public string Username { get; set; }
        public TraktUserIdsResource Ids { get; set; }
    }

    public class TraktUserIdsResource
    {
        public string Slug { get; set; }
    }
}
