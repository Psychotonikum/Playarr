using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Games.Commands
{
    public class MoveGameCommand : Command
    {
        public int SeriesId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
