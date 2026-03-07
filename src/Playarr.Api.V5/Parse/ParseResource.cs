using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Api.V5.CustomFormats;
using Playarr.Api.V5.Roms;
using Playarr.Api.V5.Game;
using Playarr.Http.REST;

namespace Playarr.Api.V5.Parse;

public class ParseResource : RestResource
{
    public string? Title { get; set; }
    public ParsedRomInfo? ParsedRomInfo { get; set; }
    public GameResource? Game { get; set; }
    public List<RomResource>? Roms { get; set; }
    public List<Language>? Languages { get; set; }
    public List<CustomFormatResource>? CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
}
