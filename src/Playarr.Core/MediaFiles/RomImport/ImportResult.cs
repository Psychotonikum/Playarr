using System.Collections.Generic;
using System.Linq;
using Playarr.Common.EnsureThat;

namespace Playarr.Core.MediaFiles.EpisodeImport
{
    public class ImportResult
    {
        public ImportDecision ImportDecision { get; private set; }
        public RomFile RomFile { get; private set; }
        public List<string> Errors { get; private set; }

        public ImportResultType Result
        {
            get
            {
                if (Errors.Any())
                {
                    if (ImportDecision.Approved)
                    {
                        return ImportResultType.Skipped;
                    }

                    return ImportResultType.Rejected;
                }

                return ImportResultType.Imported;
            }
        }

        public ImportResult(ImportDecision importDecision, params string[] errors)
        {
            Ensure.That(importDecision, () => importDecision).IsNotNull();

            ImportDecision = importDecision;
            Errors = errors.ToList();
        }

        public ImportResult(ImportDecision importDecision, RomFile romFile)
        {
            Ensure.That(importDecision, () => importDecision).IsNotNull();

            ImportDecision = importDecision;
            RomFile = romFile;
            Errors = new List<string>();
        }
    }
}
