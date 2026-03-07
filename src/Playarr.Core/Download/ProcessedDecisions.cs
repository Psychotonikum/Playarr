using System.Collections.Generic;
using Playarr.Core.DecisionEngine;

namespace Playarr.Core.Download
{
    public class ProcessedDecisions
    {
        public List<DownloadDecision> Grabbed { get; set; }
        public List<DownloadDecision> Pending { get; set; }
        public List<DownloadDecision> Rejected { get; set; }

        public ProcessedDecisions(List<DownloadDecision> grabbed, List<DownloadDecision> pending, List<DownloadDecision> rejected)
        {
            Grabbed = grabbed;
            Pending = pending;
            Rejected = rejected;
        }
    }
}
