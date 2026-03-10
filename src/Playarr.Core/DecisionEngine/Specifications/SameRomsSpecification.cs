using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public class SameEpisodesSpecification
    {
        private readonly IRomService _romService;

        public SameEpisodesSpecification(IRomService episodeService)
        {
            _romService = episodeService;
        }

        public bool IsSatisfiedBy(List<Rom> roms)
        {
            var romIds = roms.SelectList(e => e.Id);
            var romFileIds = roms.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFileId).Distinct();

            foreach (var romFileId in romFileIds)
            {
                var episodesInFile = _romService.GetRomsByFileId(romFileId);

                if (episodesInFile.Select(e => e.Id).Except(romIds).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
