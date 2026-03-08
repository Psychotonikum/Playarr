using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Games;
using Playarr.Core.RootFolders;
using Playarr.Http;

namespace Playarr.Api.V5.GameSystems;

public class ScannedGameEntryResource
{
    public string? GameTitle { get; set; }
    public int GameSystemId { get; set; }
    public string? SystemFolderName { get; set; }
    public int SystemType { get; set; }
    public bool HasBase { get; set; }
    public bool IsMissingBase { get; set; }

    public ScannedRomFileResource? BaseFile { get; set; }
    public List<ScannedRomFileResource> Updates { get; set; } = new();
    public List<ScannedRomFileResource> Dlcs { get; set; } = new();
}

public class ScannedRomFileResource
{
    public string? FullPath { get; set; }
    public string? FileName { get; set; }
    public int FileType { get; set; }
    public string? Version { get; set; }
    public string? DlcIndex { get; set; }
    public string? Region { get; set; }
    public long Size { get; set; }
}

[V5ApiController("gamesystem/scan")]
public class GameSystemScanController : Controller
{
    private readonly IGameSystemService _gameSystemService;
    private readonly ISystemFolderScanner _systemFolderScanner;
    private readonly IRootFolderService _rootFolderService;

    public GameSystemScanController(IGameSystemService gameSystemService,
                                     ISystemFolderScanner systemFolderScanner,
                                     IRootFolderService rootFolderService)
    {
        _gameSystemService = gameSystemService;
        _systemFolderScanner = systemFolderScanner;
        _rootFolderService = rootFolderService;
    }

    [HttpGet("{systemId:int}")]
    [Produces("application/json")]
    public ActionResult<List<ScannedGameEntryResource>> ScanSystem(int systemId, [FromQuery] int rootFolderId)
    {
        var system = _gameSystemService.Get(systemId);
        if (system == null)
        {
            return NotFound();
        }

        var rootFolder = _rootFolderService.Get(rootFolderId, true);
        if (rootFolder == null)
        {
            return NotFound();
        }

        var entries = _systemFolderScanner.ScanSystemFolder(rootFolder.Path, system);
        return Ok(entries.Select(MapToResource).ToList());
    }

    [HttpGet]
    [Produces("application/json")]
    public ActionResult<List<ScannedGameEntryResource>> ScanAllSystems([FromQuery] int rootFolderId)
    {
        var rootFolder = _rootFolderService.Get(rootFolderId, true);
        if (rootFolder == null)
        {
            return NotFound();
        }

        var systems = _gameSystemService.All();
        var entries = _systemFolderScanner.ScanAllSystems(rootFolder.Path, systems);
        return Ok(entries.Select(MapToResource).ToList());
    }

    private static ScannedGameEntryResource MapToResource(ScannedGameEntry entry)
    {
        return new ScannedGameEntryResource
        {
            GameTitle = entry.GameTitle,
            GameSystemId = entry.GameSystemId,
            SystemFolderName = entry.SystemFolderName,
            SystemType = (int)entry.SystemType,
            HasBase = entry.HasBase,
            IsMissingBase = entry.IsMissingBase,
            BaseFile = entry.BaseFile != null ? MapFileToResource(entry.BaseFile) : null,
            Updates = entry.Updates.Select(MapFileToResource).ToList(),
            Dlcs = entry.Dlcs.Select(MapFileToResource).ToList()
        };
    }

    private static ScannedRomFileResource MapFileToResource(ScannedRomFile file)
    {
        return new ScannedRomFileResource
        {
            FullPath = file.FullPath,
            FileName = file.FileName,
            FileType = (int)file.FileType,
            Version = file.Version,
            DlcIndex = file.DlcIndex,
            Region = file.Region,
            Size = file.Size
        };
    }
}
