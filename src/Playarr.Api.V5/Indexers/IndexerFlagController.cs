using Microsoft.AspNetCore.Mvc;
using Playarr.Core.Parser.Model;
using Playarr.Http;

namespace Playarr.Api.V5.Indexers;

[V5ApiController]
public class IndexerFlagController : Controller
{
    [HttpGet]
    public List<IndexerFlagResource> GetAll()
    {
        return Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Select(f => new IndexerFlagResource
        {
            Id = (int)f,
            Name = f.ToString()
        }).ToList();
    }
}
