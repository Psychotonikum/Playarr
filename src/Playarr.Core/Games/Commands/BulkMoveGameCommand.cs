using System;
using System.Collections.Generic;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Games.Commands
{
    public class BulkMoveGameCommand : Command
    {
        public List<BulkMoveGame> Game { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }

    public class BulkMoveGame : IEquatable<BulkMoveGame>
    {
        public int SeriesId { get; set; }
        public string SourcePath { get; set; }

        public bool Equals(BulkMoveGame other)
        {
            if (other == null)
            {
                return false;
            }

            return SeriesId.Equals(other.SeriesId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return SeriesId.Equals(((BulkMoveGame)obj).SeriesId);
        }

        public override int GetHashCode()
        {
            return SeriesId.GetHashCode();
        }
    }
}
