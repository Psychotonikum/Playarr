using Playarr.Core.DecisionEngine;
using Playarr.Core.Languages;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Games;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Release;

public class ReleaseResource : RestResource
{
    public ParsedRomInfoResource? ParsedInfo { get; set; }
    public ReleaseInfoResource? Release { get; set; }
    public ReleaseDecisionResource? Decision { get; set; }
    public ReleaseHistoryResource? History { get; set; }
    public int QualityWeight { get; set; }
    public List<Language> Languages { get; set; } = [];
    public int? MappedPlatformNumber { get; set; }
    public int[] MappedRomNumbers { get; set; } = [];
    public int[] MappedAbsoluteRomNumbers { get; set; } = [];
    public int? MappedGameId { get; set; }
    public IEnumerable<ReleaseRomResource> MappedRomInfo { get; set; } = [];
    public bool EpisodeRequested { get; set; }
    public bool DownloadAllowed { get; set; }
    public int ReleaseWeight { get; set; }
    public List<CustomFormatResource>? CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
    public AlternateTitleResource? SceneMapping { get; set; }
}

public static class ReleaseResourceMapper
{
    public static ReleaseResource ToResource(this DownloadDecision model)
    {
        var releaseInfo = model.RemoteRom.Release;
        var parsedRomInfo = model.RemoteRom.ParsedRomInfo;
        var remoteRom = model.RemoteRom;

        return new ReleaseResource
        {
            ParsedInfo = parsedRomInfo.ToResource(),
            Release = releaseInfo.ToResource(),
            Decision = new ReleaseDecisionResource(model),

            Languages = remoteRom.Languages,
            MappedGameId = remoteRom.Game?.Id,
            MappedPlatformNumber = remoteRom.Roms.FirstOrDefault()?.PlatformNumber,
            MappedRomNumbers = remoteRom.Roms.Select(v => v.EpisodeNumber).ToArray(),
            MappedAbsoluteRomNumbers = remoteRom.Roms.Where(v => v.AbsoluteEpisodeNumber.HasValue).Select(v => v.AbsoluteEpisodeNumber!.Value).ToArray(),
            MappedRomInfo = remoteRom.Roms.Select(v => new ReleaseRomResource(v)),
            EpisodeRequested = remoteRom.EpisodeRequested,
            DownloadAllowed = remoteRom.DownloadAllowed,
            CustomFormatScore = remoteRom.CustomFormatScore,
            CustomFormats = remoteRom.CustomFormats?.ToResource(false),
            SceneMapping = remoteRom.SceneMapping?.ToResource(),
        };
    }

    public static ReleaseResource MapDecision(this DownloadDecision decision, int initialWeight, QualityProfile profile)
    {
        var release = decision.ToResource();

        release.ReleaseWeight = initialWeight;

        if (release.ParsedInfo?.Quality == null)
        {
            release.QualityWeight = 0;
        }
        else
        {
            release.QualityWeight = profile.GetIndex(release.ParsedInfo.Quality.Quality).Index * 100;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Real * 10;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Version;
        }

        return release;
    }
}

public class ReleaseRomResource
{
    public int Id { get; set; }
    public int PlatformNumber { get; set; }
    public int RomNumber { get; set; }
    public int? AbsoluteEpisodeNumber { get; set; }
    public string? Title { get; set; }

    public ReleaseRomResource()
    {
    }

    public ReleaseRomResource(Rom rom)
    {
        Id = rom.Id;
        PlatformNumber = rom.PlatformNumber;
        RomNumber = rom.EpisodeNumber;
        AbsoluteEpisodeNumber = rom.AbsoluteEpisodeNumber;
        Title = rom.Title;
    }
}
