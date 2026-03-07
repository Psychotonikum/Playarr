using Playarr.Common.Messaging;
using Playarr.Core.Games;

namespace Playarr.Core.MediaFiles.Events
{
    public class SeriesScanSkippedEvent : IEvent
    {
        public Game Game { get; private set; }
        public SeriesScanSkippedReason Reason { get; set; }

        public SeriesScanSkippedEvent(Game game, SeriesScanSkippedReason reason)
        {
            Game = game;
            Reason = reason;
        }
    }

    public enum SeriesScanSkippedReason
    {
        RootFolderDoesNotExist,
        RootFolderIsEmpty,
        NeverRescanAfterRefresh,
        RescanAfterManualRefreshOnly
    }
}
