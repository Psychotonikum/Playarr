using System.Collections.Generic;
using Playarr.Core.Datastore;

namespace Playarr.Core.Games
{
    public class GameSystem : ModelBase
    {
        public GameSystem()
        {
            FileExtensions = new List<string>();
            Tags = new HashSet<int>();
        }

        public string Name { get; set; }
        public string FolderName { get; set; }
        public GameSystemType SystemType { get; set; }
        public List<string> FileExtensions { get; set; }

        // Aerofoil naming scheme per system
        public string NamingFormat { get; set; }
        public string UpdateNamingFormat { get; set; }
        public string DlcNamingFormat { get; set; }

        // Patchable system subfolder names
        public string BaseFolderName { get; set; }
        public string UpdateFolderName { get; set; }
        public string DlcFolderName { get; set; }

        public HashSet<int> Tags { get; set; }

        public override string ToString()
        {
            return $"[{Id}][{Name}]";
        }
    }
}
