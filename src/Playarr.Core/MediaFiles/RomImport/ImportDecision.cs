using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport
{
    public class ImportDecision
    {
        public LocalEpisode LocalEpisode { get; private set; }
        public IEnumerable<ImportRejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalEpisode localRom, params ImportRejection[] rejections)
        {
            LocalEpisode = localRom;
            Rejections = rejections.ToList();
        }
    }
}
