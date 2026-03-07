using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Parser.Model;

namespace Playarr.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteEpisode RemoteEpisode { get; private set; }
        public IEnumerable<DownloadRejection> Rejections { get; private set; }

        public bool Approved => !Rejections.Any();

        public bool TemporarilyRejected
        {
            get
            {
                return Rejections.Any() && Rejections.All(r => r.Type == RejectionType.Temporary);
            }
        }

        public bool Rejected
        {
            get
            {
                return Rejections.Any() && Rejections.Any(r => r.Type == RejectionType.Permanent);
            }
        }

        public DownloadDecision(RemoteEpisode rom, params DownloadRejection[] rejections)
        {
            RemoteEpisode = rom;
            Rejections = rejections.ToList();
        }

        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteEpisode;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteEpisode;
        }
    }
}
