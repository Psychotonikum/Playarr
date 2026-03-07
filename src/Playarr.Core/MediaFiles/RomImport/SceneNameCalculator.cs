using System.IO;
using Playarr.Common.Extensions;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport
{
    public static class SceneNameCalculator
    {
        public static string GetSceneName(LocalEpisode localRom)
        {
            var otherVideoFiles = localRom.OtherVideoFiles;
            var downloadClientInfo = localRom.DownloadClientRomInfo;

            if (!otherVideoFiles && downloadClientInfo != null && !downloadClientInfo.FullSeason)
            {
                return FileExtensions.RemoveFileExtension(downloadClientInfo.ReleaseTitle);
            }

            var fileName = Path.GetFileNameWithoutExtension(localRom.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            var folderTitle = localRom.FolderRomInfo?.ReleaseTitle;

            if (!otherVideoFiles &&
                localRom.FolderRomInfo?.FullSeason == false &&
                folderTitle.IsNotNullOrWhiteSpace() &&
                SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
