using Playarr.Core.DecisionEngine;

namespace Playarr.Core.MediaFiles.EpisodeImport;

public class ImportRejection : Rejection<ImportRejectionReason>
{
    public ImportRejection(ImportRejectionReason reason, string message, RejectionType type = RejectionType.Permanent)
        : base(reason, message, type)
    {
    }
}
