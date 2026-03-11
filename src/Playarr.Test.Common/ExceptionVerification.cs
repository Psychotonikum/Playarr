using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace Playarr.Test.Common
{
    public class ExceptionVerification : Target
    {
        private static readonly AsyncLocal<List<LogEventInfo>> _asyncLogs = new AsyncLocal<List<LogEventInfo>>();

        private static readonly AsyncLocal<ManualResetEventSlim> _asyncWaitEvent = new AsyncLocal<ManualResetEventSlim>();

        private static List<LogEventInfo> Logs => _asyncLogs.Value ??= new List<LogEventInfo>();
        private static ManualResetEventSlim WaitEvent => _asyncWaitEvent.Value ??= new ManualResetEventSlim();

        protected override void Write(LogEventInfo logEvent)
        {
            var logs = _asyncLogs.Value;
            if (logs == null)
            {
                return;
            }

            lock (logs)
            {
                if (logEvent.Level >= LogLevel.Warn)
                {
                    logs.Add(logEvent);
                    WaitEvent.Set();
                }
            }
        }

        public static void Reset()
        {
            _asyncLogs.Value = new List<LogEventInfo>();
            _asyncWaitEvent.Value = new ManualResetEventSlim();
        }

        public static void AssertNoUnexpectedLogs()
        {
            ExpectedFatals(0);
            ExpectedErrors(0);
            ExpectedWarns(0);
        }

        private static string GetLogsString(IEnumerable<LogEventInfo> logs)
        {
            var errors = "";
            foreach (var log in logs)
            {
                var exception = "";
                if (log.Exception != null)
                {
                    exception = string.Format("[{0}: {1}]", log.Exception.GetType(), log.Exception.Message);
                }

                errors += Environment.NewLine + string.Format("[{0}] {1}: {2} {3}", log.Level, log.LoggerName, log.FormattedMessage, exception);
            }

            return errors;
        }

        public static void WaitForErrors(int count, int msec)
        {
            var logs = Logs;
            var waitEvent = WaitEvent;

            while (true)
            {
                lock (logs)
                {
                    var levelLogs = logs.Where(l => l.Level == LogLevel.Error).ToList();

                    if (levelLogs.Count >= count)
                    {
                        break;
                    }

                    waitEvent.Reset();
                }

                if (!waitEvent.Wait(msec))
                {
                    break;
                }
            }

            Expected(LogLevel.Error, count);
        }

        public static void ExpectedErrors(int count)
        {
            Expected(LogLevel.Error, count);
        }

        public static void ExpectedFatals(int count)
        {
            Expected(LogLevel.Fatal, count);
        }

        public static void ExpectedWarns(int count)
        {
            Expected(LogLevel.Warn, count);
        }

        public static void IgnoreWarns()
        {
            Ignore(LogLevel.Warn);
        }

        public static void IgnoreErrors()
        {
            Ignore(LogLevel.Error);
        }

        public static void MarkInconclusive(Type exception)
        {
            var logs = Logs;

            lock (logs)
            {
                var inconclusiveLogs = logs.Where(l => l.Exception != null && l.Exception.GetType() == exception).ToList();

                if (inconclusiveLogs.Any())
                {
                    inconclusiveLogs.ForEach(c => logs.Remove(c));
                    Assert.Inconclusive(GetLogsString(inconclusiveLogs));
                }
            }
        }

        public static void MarkInconclusive(string text)
        {
            var logs = Logs;

            lock (logs)
            {
                var inconclusiveLogs = logs.Where(l => l.FormattedMessage.ToLower().Contains(text.ToLower())).ToList();

                if (inconclusiveLogs.Any())
                {
                    inconclusiveLogs.ForEach(c => logs.Remove(c));
                    Assert.Inconclusive(GetLogsString(inconclusiveLogs));
                }
            }
        }

        private static void Expected(LogLevel level, int count)
        {
            var logs = Logs;

            lock (logs)
            {
                var levelLogs = logs.Where(l => l.Level == level).ToList();

                if (levelLogs.Count != count)
                {
                    var message = string.Format("{0} {1}(s) were expected but {2} were logged.\n\r{3}",
                        count,
                        level,
                        levelLogs.Count,
                        GetLogsString(levelLogs));

                    message = "\n\r****************************************************************************************\n\r"
                        + message +
                        "\n\r****************************************************************************************";

                    Assert.Fail(message);
                }

                levelLogs.ForEach(c => logs.Remove(c));
            }
        }

        private static void Ignore(LogLevel level)
        {
            var logs = Logs;

            lock (logs)
            {
                var levelLogs = logs.Where(l => l.Level == level).ToList();
                levelLogs.ForEach(c => logs.Remove(c));
            }
        }
    }
}
