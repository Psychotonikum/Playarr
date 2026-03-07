using System.Text;
using NLog;
using NLog.Layouts;
using Playarr.Common.EnvironmentInfo;

namespace Playarr.Common.Instrumentation;

public class CleansingConsoleLogLayout(string format)
    : SimpleLayout(format)
{
    protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
    {
        base.RenderFormattedMessage(logEvent, target);

        if (RuntimeInfo.IsProduction)
        {
            var result = CleanseLogMessage.Cleanse(target.ToString());
            target.Clear();
            target.Append(result);
        }
    }
}
