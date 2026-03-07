namespace Playarr.Core.Parser.Model
{
    public class GameTitleInfo
    {
        public string Title { get; set; }
        public string TitleWithoutYear { get; set; }
        public int Year { get; set; }
        public string[] AllTitles { get; set; }
    }
}
