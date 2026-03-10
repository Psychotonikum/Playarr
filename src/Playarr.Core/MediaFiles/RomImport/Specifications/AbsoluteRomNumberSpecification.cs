using NLog;
using Playarr.Core.Download;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AbsoluteRomNumberSpecification : IImportDecisionEngineSpecification
    {
        private readonly IBuildFileNames _buildFileNames;
        private readonly Logger _logger;

        public AbsoluteRomNumberSpecification(IBuildFileNames buildFileNames, Logger logger)
        {
            _buildFileNames = buildFileNames;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            // Absolute rom numbers are not applicable to game ROMs
            return ImportSpecDecision.Accept();
        }
    }
}
