namespace Playarr.Core.Games
{
    public class AddGameOptions : MonitoringOptions
    {
        public bool SearchForMissingEpisodes { get; set; }
        public bool SearchForCutoffUnmetEpisodes { get; set; }
    }
}
