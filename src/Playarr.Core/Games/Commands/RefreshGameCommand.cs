using System.Collections.Generic;
using System.Text.Json.Serialization;
using Playarr.Common.Extensions;
using Playarr.Core.Messaging.Commands;

namespace Playarr.Core.Games.Commands
{
    public class RefreshSeriesCommand : Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int GameId
        {
            get => 0;
            set
            {
                if (GameIds.Empty())
                {
                    GameIds.Add(value);
                }
            }
        }

        public List<int> GameIds { get; set; }
        public bool IsNewSeries { get; set; }

        public RefreshSeriesCommand()
        {
            GameIds = new List<int>();
        }

        public RefreshSeriesCommand(List<int> gameIds, bool isNewSeries = false)
        {
            GameIds = gameIds;
            IsNewSeries = isNewSeries;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => GameIds.Empty();

        public override bool IsLongRunning => true;

        public override string CompletionMessage => "Completed";
    }
}
