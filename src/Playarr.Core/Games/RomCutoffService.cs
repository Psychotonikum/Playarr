using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;

namespace Playarr.Core.Games
{
    public interface IEpisodeCutoffService
    {
        PagingSpec<Rom> EpisodesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec);
    }

    public class EpisodeCutoffService : IEpisodeCutoffService
    {
        private readonly IRomRepository _episodeRepository;
        private readonly IQualityProfileService _qualityProfileService;

        public EpisodeCutoffService(IRomRepository episodeRepository, IQualityProfileService qualityProfileService)
        {
            _episodeRepository = episodeRepository;
            _qualityProfileService = qualityProfileService;
        }

        public PagingSpec<Rom> EpisodesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _qualityProfileService.All();

            // Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoff = profile.UpgradeAllowed ? profile.Cutoff : profile.FirststAllowedQuality().Id;
                var cutoffIndex = profile.GetIndex(cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            if (qualitiesBelowCutoff.Empty())
            {
                pagingSpec.Records = new List<Rom>();

                return pagingSpec;
            }

            return _episodeRepository.EpisodesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, false);
        }
    }
}
