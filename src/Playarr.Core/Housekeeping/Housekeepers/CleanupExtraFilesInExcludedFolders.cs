using System.IO;
using System.Linq;
using Playarr.Core.Extras.Files;
using Playarr.Core.Extras.Others;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Housekeeping.Housekeepers
{
    public class CleanupExtraFilesInExcludedFolders : IHousekeepingTask
    {
        private readonly IExtraFileRepository<OtherExtraFile> _extraFileRepository;
        private readonly IGameService _gameService;
        private readonly IDiskScanService _diskScanService;

        public CleanupExtraFilesInExcludedFolders(IExtraFileRepository<OtherExtraFile> extraFileRepository, IGameService seriesService, IDiskScanService diskScanService)
        {
            _extraFileRepository = extraFileRepository;
            _gameService = seriesService;
            _diskScanService = diskScanService;
        }

        public void Clean()
        {
            var allGames = _gameService.GetAllGames();

            foreach (var game in allGames)
            {
                var extraFiles = _extraFileRepository.GetFilesBySeries(game.Id);
                var filteredExtraFiles = _diskScanService.FilterPaths(game.Path, extraFiles.Select(e => Path.Combine(game.Path, e.RelativePath)));

                if (filteredExtraFiles.Count == extraFiles.Count)
                {
                    continue;
                }

                var excludedExtraFiles = extraFiles.Where(e => !filteredExtraFiles.Contains(Path.Combine(e.RelativePath))).ToList();

                if (excludedExtraFiles.Any())
                {
                    _extraFileRepository.DeleteMany(excludedExtraFiles.Select(e => e.Id));
                }
            }
        }
    }
}
