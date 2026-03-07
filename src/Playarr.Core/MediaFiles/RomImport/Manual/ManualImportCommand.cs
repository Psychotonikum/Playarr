using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.MediaFiles.EpisodeImport.Manual
{
    public class ManualImportCommand : Command
    {
        public List<ManualImportFile> Files { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public ImportMode ImportMode { get; set; }
    }
}
