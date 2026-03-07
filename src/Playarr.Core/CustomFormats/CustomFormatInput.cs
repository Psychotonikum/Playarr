using System.Collections.Generic;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.CustomFormats
{
    public class CustomFormatInput
    {
        public ParsedRomInfo RomInfo { get; set; }
        public Game Game { get; set; }
        public long Size { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public List<Language> Languages { get; set; }
        public string Filename { get; set; }
        public ReleaseType ReleaseType { get; set; }

        public CustomFormatInput()
        {
            Languages = new List<Language>();
        }

        // public CustomFormatInput(ParsedRomInfo romInfo, Game game)
        // {
        //     RomInfo = romInfo;
        //     Game = game;
        // }
        //
        // public CustomFormatInput(ParsedRomInfo romInfo, Game game, long size, List<Language> languages)
        // {
        //     RomInfo = romInfo;
        //     Game = game;
        //     Size = size;
        //     Languages = languages;
        // }
        //
        // public CustomFormatInput(ParsedRomInfo romInfo, Game game, long size, List<Language> languages, string filename)
        // {
        //     RomInfo = romInfo;
        //     Game = game;
        //     Size = size;
        //     Languages = languages;
        //     Filename = filename;
        // }
    }
}
