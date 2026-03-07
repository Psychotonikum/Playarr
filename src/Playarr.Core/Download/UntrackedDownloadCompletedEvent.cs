using System.Collections.Generic;
using Playarr.Common.Messaging;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Download
{
    public class UntrackedDownloadCompletedEvent : IEvent
    {
        public Game Game { get; private set; }
        public List<Rom> Roms { get; private set; }
        public List<RomFile> RomFiles { get; private set; }
        public ParsedRomInfo ParsedRomInfo { get; private set; }
        public string SourcePath { get; private set; }

        public UntrackedDownloadCompletedEvent(Game game, List<Rom> roms, List<RomFile> romFiles, ParsedRomInfo parsedRomInfo, string sourcePath)
        {
            Game = game;
            Roms = roms;
            RomFiles = romFiles;
            ParsedRomInfo = parsedRomInfo;
            SourcePath = sourcePath;
        }
    }
}
