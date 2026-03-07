using System.Collections.Generic;
using System.IO;
using Playarr.Common.Extensions;
using Playarr.Core.Download;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateEpisodes : IAggregateLocalEpisode
    {
        public int Order => 1;

        private readonly IParsingService _parsingService;

        public AggregateEpisodes(IParsingService parsingService)
        {
            _parsingService = parsingService;
        }

        public LocalEpisode Aggregate(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            localRom.Roms = GetEpisodes(localRom);

            return localRom;
        }

        private ParsedRomInfo GetBestRomInfo(LocalEpisode localRom)
        {
            var parsedRomInfo = localRom.FileRomInfo;
            var downloadClientRomInfo = localRom.DownloadClientRomInfo;
            var folderRomInfo = localRom.FolderRomInfo;

            if (!localRom.OtherVideoFiles && !SceneChecker.IsSceneTitle(Path.GetFileNameWithoutExtension(localRom.Path)))
            {
                if (downloadClientRomInfo != null &&
                    !downloadClientRomInfo.FullSeason &&
                    PreferOtherRomInfo(parsedRomInfo, downloadClientRomInfo))
                {
                    parsedRomInfo = localRom.DownloadClientRomInfo;
                }
                else if (folderRomInfo != null &&
                         !folderRomInfo.FullSeason &&
                         PreferOtherRomInfo(parsedRomInfo, folderRomInfo))
                {
                    parsedRomInfo = localRom.FolderRomInfo;
                }
            }

            if (parsedRomInfo == null)
            {
                parsedRomInfo = GetSpecialRomInfo(localRom, parsedRomInfo);
            }

            return parsedRomInfo;
        }

        private ParsedRomInfo GetSpecialRomInfo(LocalEpisode localRom, ParsedRomInfo parsedRomInfo)
        {
            var title = Path.GetFileNameWithoutExtension(localRom.Path);
            var specialRomInfo = _parsingService.ParseSpecialRomTitle(parsedRomInfo, title, localRom.Game);

            return specialRomInfo;
        }

        private List<Rom> GetEpisodes(LocalEpisode localRom)
        {
            var bestRomInfoForEpisodes = GetBestRomInfo(localRom);
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localRom.Path));

            if (bestRomInfoForEpisodes == null)
            {
                return new List<Rom>();
            }

            if (ValidateParsedRomInfo.ValidateForGameType(bestRomInfoForEpisodes, localRom.Game, isMediaFile))
            {
                var roms = _parsingService.GetEpisodes(bestRomInfoForEpisodes, localRom.Game, localRom.SceneSource);

                if (roms.Empty() && bestRomInfoForEpisodes.IsPossibleSpecialEpisode)
                {
                    var parsedSpecialRomInfo = GetSpecialRomInfo(localRom, bestRomInfoForEpisodes);

                    if (parsedSpecialRomInfo != null)
                    {
                        roms = _parsingService.GetEpisodes(parsedSpecialRomInfo, localRom.Game, localRom.SceneSource);
                    }
                }

                return roms;
            }

            return new List<Rom>();
        }

        private bool PreferOtherRomInfo(ParsedRomInfo fileRomInfo, ParsedRomInfo otherRomInfo)
        {
            if (fileRomInfo == null)
            {
                return true;
            }

            // When the files rom info is not absolute prefer it over a parsed rom info that is absolute
            if (!fileRomInfo.IsAbsoluteNumbering && otherRomInfo.IsAbsoluteNumbering)
            {
                return false;
            }

            return true;
        }
    }
}
