using System.Linq;
using Playarr.Core.Parser;
using Playarr.Core.Games;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForSeries : IHousekeepingTask
    {
        private readonly IGameRepository _seriesRepository;

        public UpdateCleanTitleForSeries(IGameRepository seriesRepository)
        {
            _seriesRepository = seriesRepository;
        }

        public void Clean()
        {
            var game = _seriesRepository.All().ToList();

            game.ForEach(s =>
            {
                var cleanTitle = s.Title.CleanGameTitle();
                if (s.CleanTitle != cleanTitle)
                {
                    s.CleanTitle = cleanTitle;
                    _seriesRepository.Update(s);
                }
            });
        }
    }
}
