using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Qualities.Commands
{
    public class ResetQualityDefinitionsCommand : Command
    {
        public bool ResetTitles { get; set; }

        public ResetQualityDefinitionsCommand(bool resetTitles = false)
        {
            ResetTitles = resetTitles;
        }
    }
}
