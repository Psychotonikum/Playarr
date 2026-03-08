using System;
using System.Collections.Generic;
using System.IO;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Qualities;
using Playarr.Api.V3.CustomFormats;
using Playarr.Http.REST;

namespace Playarr.Api.V3.RomFiles
{
    public class RomFileResource : RestResource
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int? IndexerFlags { get; set; }
        public ReleaseType? ReleaseType { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class RomFileResourceMapper
    {
        public static RomFileResource ToResource(this RomFile model, Playarr.Core.Games.Game game, IUpgradableSpecification upgradableSpecification, ICustomFormatCalculationService formatCalculationService)
        {
            if (model == null)
            {
                return null;
            }

            model.Game = game;
            var customFormats = formatCalculationService?.ParseCustomFormat(model, model.Game);
            var customFormatScore = game?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new RomFileResource
            {
                Id = model.Id,

                GameId = model.GameId,
                PlatformNumber = model.PlatformNumber,
                RelativePath = model.RelativePath,
                Path = Path.Combine(game.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                ReleaseGroup = model.ReleaseGroup,
                Languages = model.Languages,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                QualityCutoffNotMet = upgradableSpecification.QualityCutoffNotMet(game.QualityProfile.Value, model.Quality),
                CustomFormats = customFormats.ToResource(false),
                CustomFormatScore = customFormatScore,
                IndexerFlags = (int)model.IndexerFlags,
                ReleaseType = model.ReleaseType,
            };
        }
    }
}
