using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class SeriesDeleteMessage
    {
        public string Message { get; set; }
        public Game Game { get; set; }
        public bool DeletedFiles { get; set; }
        public string DeletedFilesMessage { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public SeriesDeleteMessage(Game game, bool deleteFiles)
        {
            Game = game;
            DeletedFiles = deleteFiles;
            DeletedFilesMessage = DeletedFiles ?
                "Game removed and all files were deleted" :
                "Game removed, files were not deleted";
            Message = game.Title + " - " + DeletedFilesMessage;
        }
    }
}
