namespace Playarr.Api.V5.Game;

public class GameEditorDeleteResource
{
    public List<int> GameIds { get; set; } = [];
    public bool DeleteFiles { get; set; }
}
