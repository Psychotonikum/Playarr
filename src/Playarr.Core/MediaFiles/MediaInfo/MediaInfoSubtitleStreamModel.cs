using Playarr.Core.Datastore;

namespace Playarr.Core.MediaFiles.MediaInfo;

public class MediaInfoSubtitleStreamModel : IEmbeddedDocument
{
    public string Language { get; set; }
    public string Format { get; set; }
    public string Title { get; set; }
    public bool? Forced { get; set; }
    public bool? HearingImpaired { get; set; }
}
