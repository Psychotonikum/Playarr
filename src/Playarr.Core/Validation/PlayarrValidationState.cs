namespace Playarr.Core.Validation
{
    public class PlayarrValidationState
    {
        public static PlayarrValidationState Warning = new PlayarrValidationState { IsWarning = true };

        public bool IsWarning { get; set; }
    }
}
