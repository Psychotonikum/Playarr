using System.Collections.Generic;
using Playarr.Core.CustomFormats;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Qualities;
using Playarr.Core.Games;

namespace Playarr.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardSample(NamingConfig nameSpec);
        SampleResult GetMultiEpisodeSample(NamingConfig nameSpec);
        string GetGameFolderSample(NamingConfig nameSpec);
        string GetPlatformFolderSample(NamingConfig nameSpec);
        string GetSpecialsFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private static Game _standardSeries;
        private static Rom _episode1;
        private static Rom _episode2;
        private static Rom _episode3;
        private static List<Rom> _singleEpisode;
        private static List<Rom> _multiEpisodes;
        private static RomFile _singleRomFile;
        private static RomFile _multiRomFile;
        private static List<CustomFormat> _customFormats;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardSeries = new Game
            {
                SeriesType = GameTypes.Standard,
                Title = "The Game Title's!",
                Year = 2010,
                ImdbId = "tt12345",
                IgdbId = 12345,
                RawgId = 54321,
                TmdbId = 11223
            };

            _episode1 = new Rom
            {
                PlatformNumber = 1,
                EpisodeNumber = 1,
                Title = "Rom Title (1)",
                AirDate = "2013-10-30",
                AbsoluteEpisodeNumber = 1,
            };

            _episode2 = new Rom
            {
                PlatformNumber = 1,
                EpisodeNumber = 2,
                Title = "Rom Title (2)",
                AbsoluteEpisodeNumber = 2
            };

            _episode3 = new Rom
            {
                PlatformNumber = 1,
                EpisodeNumber = 3,
                Title = "Rom Title (3)",
                AbsoluteEpisodeNumber = 3
            };

            _singleEpisode = new List<Rom> { _episode1 };
            _multiEpisodes = new List<Rom> { _episode1, _episode2, _episode3 };

            var mediaInfo = new MediaInfoModel
            {
                VideoFormat = "AVC",
                VideoBitDepth = 10,
                VideoColourPrimaries = "bt2020",
                VideoTransferCharacteristics = "HLG",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Language = "ger",
                        Format = "dts",
                        Channels = 6,
                        ChannelPositions = "5.1",
                    }
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "eng" },
                    new MediaInfoSubtitleStreamModel { Language = "ger" }
                ],
            };

            _customFormats = new List<CustomFormat>
            {
                new CustomFormat
                {
                    Name = "Surround Sound",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat
                {
                    Name = "x264",
                    IncludeCustomFormatWhenRenaming = true
                }
            };

            _singleRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                RelativePath = "The.Game.Title's!.S01E01.1080p.WEBDL.x264-EVOLVE.mkv",
                SceneName = "The.Game.Title's!.S01E01.1080p.WEBDL.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

            _multiRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                RelativePath = "The.Game.Title's!.S01E01-E03.1080p.WEBDL.x264-EVOLVE.mkv",
                SceneName = "The.Game.Title's!.S01E01-E03.1080p.WEBDL.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo,
            };
        }

        public SampleResult GetStandardSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _standardSeries, _singleRomFile, nameSpec, _customFormats),
                Game = _standardSeries,
                Roms = _singleEpisode,
                RomFile = _singleRomFile
            };

            return result;
        }

        public SampleResult GetMultiEpisodeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_multiEpisodes, _standardSeries, _multiRomFile, nameSpec, _customFormats),
                Game = _standardSeries,
                Roms = _multiEpisodes,
                RomFile = _multiRomFile
            };

            return result;
        }

        public string GetGameFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetGameFolder(_standardSeries, nameSpec);
        }

        public string GetPlatformFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetPlatformFolder(_standardSeries, _episode1.PlatformNumber, nameSpec);
        }

        public string GetSpecialsFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetPlatformFolder(_standardSeries, 0, nameSpec);
        }

        private string BuildSample(List<Rom> roms, Game game, RomFile romFile, NamingConfig nameSpec, List<CustomFormat> customFormats)
        {
            try
            {
                return _buildFileNames.BuildFileName(roms, game, romFile, "", nameSpec, customFormats);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
