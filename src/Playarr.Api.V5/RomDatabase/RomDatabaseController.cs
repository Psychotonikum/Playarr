using Microsoft.AspNetCore.Mvc;
using Playarr.Core.MetadataSource.RomDatabase;
using Playarr.Http;

namespace Playarr.Api.V5.RomDatabase
{
    [V5ApiController]
    public class RomDatabaseController : Controller
    {
        private readonly IRomDatabaseService _romDatabaseService;

        public RomDatabaseController(IRomDatabaseService romDatabaseService)
        {
            _romDatabaseService = romDatabaseService;
        }

        [HttpGet("systems")]
        [Produces("application/json")]
        public List<RomDatabaseSystem> GetSystems()
        {
            return _romDatabaseService.GetAvailableSystems();
        }

        [HttpGet("systems/{systemId}/status")]
        [Produces("application/json")]
        public RomDatabaseStatus GetSystemStatus(string systemId)
        {
            return _romDatabaseService.GetSystemStatus(systemId);
        }

        [HttpPost("systems/{systemId}/download")]
        public IActionResult DownloadDatabase(string systemId)
        {
            _romDatabaseService.DownloadDatabase(systemId);
            return Ok(new { message = $"Database info logged. Place DAT files in the rom_databases folder." });
        }

        [HttpGet("lookup/hash")]
        [Produces("application/json")]
        public ActionResult<RomDatabaseEntry> LookupByHash([FromQuery] string systemId, [FromQuery] string hash)
        {
            var result = _romDatabaseService.LookupByHash(systemId, hash);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpGet("lookup/name")]
        [Produces("application/json")]
        public ActionResult<RomDatabaseEntry> LookupByName([FromQuery] string systemId, [FromQuery] string filename)
        {
            var result = _romDatabaseService.LookupByName(systemId, filename);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }
    }
}
