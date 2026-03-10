using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine.Specifications
{
    public interface IDownloadDecisionEngineSpecification
    {
        RejectionType Type { get; }

        SpecificationPriority Priority { get; }

        DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information);
    }
}
