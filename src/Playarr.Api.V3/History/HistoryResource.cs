using System;
using System.Collections.Generic;
using Playarr.Core.CustomFormats;
using Playarr.Core.History;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Roms;
using Playarr.Api.V3.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V3.History
{
    public class HistoryResource : RestResource
    {
        public int EpisodeId { get; set; }
        public int GameId { get; set; }
        public string SourceTitle { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public DateTime Date { get; set; }
        public string DownloadId { get; set; }

        public EpisodeHistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public RomResource Rom { get; set; }
        public GameResource Game { get; set; }
    }

    public static class HistoryResourceMapper
    {
        public static HistoryResource ToResource(this EpisodeHistory model, ICustomFormatCalculationService formatCalculator)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = formatCalculator.ParseCustomFormat(model, model.Game);
            var customFormatScore = model.Game.QualityProfile.Value.CalculateCustomFormatScore(customFormats);

            return new HistoryResource
            {
                Id = model.Id,

                EpisodeId = model.EpisodeId,
                GameId = model.GameId,
                SourceTitle = model.SourceTitle,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats.ToResource(false),
                CustomFormatScore = customFormatScore,

                // QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data = model.Data

                // Rom
                // Game
            };
        }
    }
}
