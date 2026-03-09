using System;
using System.Collections.Generic;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles
{
    public class RomFile : ModelBase
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string OriginalFilePath { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public QualityModel Quality { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public LazyLoaded<List<Rom>> Roms { get; set; }
        public LazyLoaded<Game> Game { get; set; }
        public List<Language> Languages { get; set; }
        public string Region { get; set; }
        public string CrcHash { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public RomFileType RomFileType { get; set; }
        public string PatchVersion { get; set; }
        public string DlcIndex { get; set; }
        public int? LinkedGameId { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, RelativePath);
        }

        public string GetSceneOrFileName()
        {
            if (SceneName.IsNotNullOrWhiteSpace())
            {
                return SceneName;
            }

            if (RelativePath.IsNotNullOrWhiteSpace())
            {
                return System.IO.Path.GetFileNameWithoutExtension(RelativePath);
            }

            if (Path.IsNotNullOrWhiteSpace())
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }

            return string.Empty;
        }
    }
}
