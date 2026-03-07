using NLog;
using Playarr.Core.Download;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public NotSampleSpecification(IDetectSample detectSample,
                                      Logger logger)
        {
            _detectSample = detectSample;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return ImportSpecDecision.Accept();
            }

            try
            {
                var sample = _detectSample.IsSample(localRom);

                if (sample == DetectSampleResult.Sample)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.Sample, "Sample");
                }
                else if (sample == DetectSampleResult.Indeterminate)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.SampleIndeterminate, "Unable to determine if file is a sample");
                }
            }
            catch (InvalidSeasonException e)
            {
                _logger.Warn(e, "Invalid platform detected during sample check");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
