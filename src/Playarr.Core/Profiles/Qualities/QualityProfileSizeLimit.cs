using Playarr.Core.Qualities;

namespace Playarr.Core.Profiles.Qualities;

public class QualityProfileSizeLimit
{
    public Quality Quality { get; set; }
    public double? MinSize { get; set; }
    public double? MaxSize { get; set; }
    public double? PreferredSize { get; set; }

    public QualityProfileSizeLimit(QualityDefinition qualityDefinition)
    {
        Quality = qualityDefinition.Quality;
        MinSize = qualityDefinition.MinSize;
        MaxSize = qualityDefinition.MaxSize;
        PreferredSize = qualityDefinition.PreferredSize;
    }
}
