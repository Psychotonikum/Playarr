using System.Collections.Generic;
using Playarr.Core.Extras.Files;
using Playarr.Core.Games;

namespace Playarr.Core.Extras
{
    public interface IImportExistingExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> ProcessFiles(Game game, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename);
    }
}
