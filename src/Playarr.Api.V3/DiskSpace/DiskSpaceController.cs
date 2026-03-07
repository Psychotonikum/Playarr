using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Playarr.Core.DiskSpace;
using Playarr.Http;

namespace Playarr.Api.V3.DiskSpace
{
    [V3ApiController("diskspace")]
    public class DiskSpaceController : Controller
    {
        private readonly IDiskSpaceService _diskSpaceService;

        public DiskSpaceController(IDiskSpaceService diskSpaceService)
        {
            _diskSpaceService = diskSpaceService;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<DiskSpaceResource> GetFreeSpace()
        {
            return _diskSpaceService.GetFreeSpace().ConvertAll(DiskSpaceResourceMapper.MapToResource);
        }
    }
}
