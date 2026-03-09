using System.Collections.Generic;

namespace Playarr.Core.MetadataSource.RomDatabase
{
    public interface IRomDatabaseService
    {
        List<RomDatabaseSystem> GetAvailableSystems();
        RomDatabaseStatus GetSystemStatus(string systemId);
        void DownloadDatabase(string systemId);
        RomDatabaseEntry LookupByHash(string systemId, string hash);
        RomDatabaseEntry LookupByName(string systemId, string filename);
    }

    public class RomDatabaseSystem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public long EstimatedSizeBytes { get; set; }
        public bool IsDownloaded { get; set; }
        public int EntryCount { get; set; }
    }

    public class RomDatabaseStatus
    {
        public string SystemId { get; set; }
        public bool IsDownloaded { get; set; }
        public int EntryCount { get; set; }
        public string LastUpdated { get; set; }
    }

    public class RomDatabaseEntry
    {
        public string Name { get; set; }
        public string System { get; set; }
        public string Region { get; set; }
        public string Hash { get; set; }
        public string Source { get; set; }
        public int? TrackCount { get; set; }
    }
}
