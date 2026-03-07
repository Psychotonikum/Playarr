using Playarr.Core.Extras.Files;

namespace Playarr.Core.Extras.Metadata.Files
{
    public class MetadataFile : ExtraFile
    {
        public string Hash { get; set; }
        public string Consumer { get; set; }
        public MetadataType Type { get; set; }
    }
}
