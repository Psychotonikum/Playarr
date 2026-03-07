using NLog;
using Playarr.Core.Download;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class HasAudioTrackSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public HasAudioTrackSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.MediaInfo == null)
            {
                _logger.Debug("Failed to get media info from the file, make sure ffprobe is available, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (localRom.MediaInfo.AudioStreams == null || localRom.MediaInfo.AudioStreams.Count == 0)
            {
                _logger.Debug("No audio tracks found in file");

                return ImportSpecDecision.Reject(ImportRejectionReason.NoAudio, "No audio tracks detected");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
