using System.Collections.Generic;

namespace Playarr.Core.Update
{
    public class UpdateChanges
    {
        public List<string> New { get; set; }
        public List<string> Fixed { get; set; }

        public UpdateChanges()
        {
            New = new List<string>();
            Fixed = new List<string>();
        }
    }
}
