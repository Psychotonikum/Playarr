using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.Download;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Parser.Model
{
    public class LocalEpisode
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedRomInfo FileRomInfo { get; set; }
        public ParsedRomInfo DownloadClientRomInfo { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public ParsedRomInfo FolderRomInfo { get; set; }
        public Game Game { get; set; }
        public List<Rom> Roms { get; set; } = new();
        public List<DeletedRomFile> OldFiles { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; } = new();
        public IndexerFlags IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string SceneName { get; set; }
        public bool OtherVideoFiles { get; set; }
        public List<CustomFormat> CustomFormats { get; set; } = new();
        public int CustomFormatScore { get; set; }
        public List<CustomFormat> OriginalFileNameCustomFormats { get; set; } = new();
        public int OriginalFileNameCustomFormatScore { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public bool ScriptImported { get; set; }
        public string FileNameBeforeRename { get; set; }
        public string FileNameUsedForCustomFormatCalculation { get; set; }
        public bool ShouldImportExtras { get; set; }
        public List<string> PossibleExtraFiles { get; set; }
        public SubtitleTitleInfo SubtitleInfo { get; set; }

        public int SeasonNumber
        {
            get
            {
                var platforms = Roms.Select(c => c.SeasonNumber).Distinct().ToList();

                if (platforms.Empty())
                {
                    throw new InvalidSeasonException("Expected one platform, but found none");
                }

                if (platforms.Count > 1)
                {
                    throw new InvalidSeasonException("Expected one platform, but found {0} ({1})", platforms.Count, string.Join(", ", platforms));
                }

                return platforms.Single();
            }
        }

        public bool IsSpecial => SeasonNumber == 0;

        public override string ToString()
        {
            return Path;
        }

        public string GetSceneOrFileName()
        {
            if (SceneName.IsNotNullOrWhiteSpace())
            {
                return SceneName;
            }

            if (Path.IsNotNullOrWhiteSpace())
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }

            return string.Empty;
        }

        public RomFile ToRomFile()
        {
            var romFile = new RomFile
            {
                DateAdded = DateTime.UtcNow,
                SeriesId = Game.Id,
                Path = Path.CleanFilePath(),
                Quality = Quality,
                MediaInfo = MediaInfo,
                Game = Game,
                SeasonNumber = SeasonNumber,
                Roms = Roms,
                ReleaseGroup = ReleaseGroup,
                ReleaseHash = ReleaseHash,
                Languages = Languages,
                IndexerFlags = IndexerFlags,
                ReleaseType = ReleaseType,
                SceneName = SceneName,
                OriginalFilePath = GetOriginalFilePath()
            };

            if (Game.Path.IsParentPath(romFile.Path))
            {
                romFile.RelativePath = Game.Path.GetRelativePath(Path.CleanFilePath());
            }

            if (romFile.ReleaseType == ReleaseType.Unknown)
            {
                romFile.ReleaseType = DownloadClientRomInfo?.ReleaseType ??
                                          FolderRomInfo?.ReleaseType ??
                                          FileRomInfo?.ReleaseType ??
                                          ReleaseType.Unknown;
            }

            return romFile;
        }

        private string GetOriginalFilePath()
        {
            if (FolderRomInfo != null)
            {
                var folderPath = Path.GetAncestorPath(FolderRomInfo.ReleaseTitle);

                if (folderPath != null)
                {
                    return folderPath.GetParentPath().GetRelativePath(Path);
                }
            }

            var parentPath = Path.GetParentPath();
            var grandparentPath = parentPath.GetParentPath();

            if (grandparentPath != null)
            {
                return grandparentPath.GetRelativePath(Path);
            }

            return System.IO.Path.GetFileName(Path);
        }
    }
}
