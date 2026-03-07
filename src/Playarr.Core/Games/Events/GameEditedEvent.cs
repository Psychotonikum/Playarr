using Playarr.Common.Messaging;

namespace Playarr.Core.Games.Events
{
    public class SeriesEditedEvent : IEvent
    {
        public Game Game { get; private set; }
        public Game OldSeries { get; private set; }
        public bool EpisodesChanged { get; private set; }

        public SeriesEditedEvent(Game game, Game oldSeries, bool episodesChanged = false)
        {
            Game = game;
            OldSeries = oldSeries;
            EpisodesChanged = episodesChanged;
        }
    }
}
