using System.Collections.Generic;

namespace Playarr.Api.V3.Game
{
    public class GameEditorDeleteResource
    {
        public List<int> GameIds { get; set; }
        public bool DeleteFiles { get; set; }
    }
}
