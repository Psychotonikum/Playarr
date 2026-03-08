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
        SampleResult GetDailySample(NamingConfig nameSpec);
        SampleResult GetAnimeSample(NamingConfig nameSpec);
        SampleResult GetAnimeMultiEpisodeSample(NamingConfig nameSpec);
        string GetGameFolderSample(NamingConfig nameSpec);
        string GetPlatformFolderSample(NamingConfig nameSpec);
        string GetSpecialsFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private static Game _standardSeries;
        private static Game _dailySeries;
        private static Game _animeSeries;
        private static Rom _episode1;
        private static Rom _episode2;
        private static Rom _episode3;
        private static List<Rom> _singleEpisode;
        private static List<Rom> _multiEpisodes;
        private static RomFile _singleRomFile;
        private static RomFile _multiRomFile;
        private static RomFile _dailyRomFile;
        private static RomFile _animeRomFile;
        private static RomFile _animeMultiRomFile;
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

            _dailySeries = new Game
            {
                SeriesType = GameTypes.Daily,
                Title = "The Game Title's!",
                Year = 2010,
                ImdbId = "tt12345",
                IgdbId = 12345,
                RawgId = 54321,
                TmdbId = 11223
            };

            _animeSeries = new Game
            {
                SeriesType = GameTypes.Anime,
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

            var mediaInfoAnime = new MediaInfoModel
            {
                VideoFormat = "AVC",
                VideoBitDepth = 10,
                VideoColourPrimaries = "BT.2020",
                VideoTransferCharacteristics = "HLG",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Language = "jpn",
                        Format = "dts",
                        Channels = 6,
                        ChannelPositions = "5.1",
                    }
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "jpn" },
                    new MediaInfoSubtitleStreamModel { Language = "eng" }
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

            _dailyRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                RelativePath = "The.Game.Title's!.2013.10.30.1080p.WEBDL.x264-EVOLVE.mkv",
                SceneName = "The.Game.Title's!.2013.10.30.1080p.WEBDL.x264-EVOLVE",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

            _animeRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                RelativePath = "[RlsGroup] The Game Title's! - 001 [1080P].mkv",
                SceneName = "[RlsGroup] The Game Title's! - 001 [1080P]",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfoAnime
            };

            _animeMultiRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2)),
                RelativePath = "[RlsGroup] The Game Title's! - 001 - 103 [1080p].mkv",
                SceneName = "[RlsGroup] The Game Title's! - 001 - 103 [1080p]",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfoAnime
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

        public SampleResult GetDailySample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _dailySeries, _dailyRomFile, nameSpec, _customFormats),
                Game = _dailySeries,
                Roms = _singleEpisode,
                RomFile = _dailyRomFile
            };

            return result;
        }

        public SampleResult GetAnimeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleEpisode, _animeSeries, _animeRomFile, nameSpec, _customFormats),
                Game = _animeSeries,
                Roms = _singleEpisode,
                RomFile = _animeRomFile
            };

            return result;
        }

        public SampleResult GetAnimeMultiEpisodeSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_multiEpisodes, _animeSeries, _animeMultiRomFile, nameSpec, _customFormats),
                Game = _animeSeries,
                Roms = _multiEpisodes,
                RomFile = _animeMultiRomFile
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
