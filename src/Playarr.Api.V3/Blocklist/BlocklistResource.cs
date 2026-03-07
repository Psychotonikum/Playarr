using System;
using System.Collections.Generic;
using Playarr.Core.CustomFormats;
using Playarr.Core.Indexers;
using Playarr.Core.Languages;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Api.V3.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V3.Blocklist
{
    public class BlocklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> RomIds { get; set; }
        public string SourceTitle { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public GameResource Game { get; set; }
    }

    public static class BlocklistResourceMapper
    {
        public static BlocklistResource MapToResource(this Playarr.Core.Blocklisting.Blocklist model, ICustomFormatCalculationService formatCalculator)
        {
            if (model == null)
            {
                return null;
            }

            return new BlocklistResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                RomIds = model.RomIds,
                SourceTitle = model.SourceTitle,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = formatCalculator.ParseCustomFormat(model, model.Game).ToResource(false),
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Game = model.Game.ToResource()
            };
        }
    }
}
