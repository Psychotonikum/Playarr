using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Api.V3.Game;

namespace Playarr.Integration.Test
{
    [TestFixture]
    public class HttpLogFixture : IntegrationTest
    {
        [Test]
        public void should_log_on_error()
        {
            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);

            var resultGet = Game.All();

            var logFile = "playarr.trace.txt";
            var logLines = Logs.GetLogFileLines(logFile);

            var resultPost = Game.InvalidPost(new GameResource());

            // Skip 2 and 1 to ignore the logs endpoint
            logLines = Logs.GetLogFileLines(logFile).Skip(logLines.Length + 2).ToArray();
            Array.Resize(ref logLines, logLines.Length - 1);

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req") && v.Contains("/api/v3/game/"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res") && v.Contains("/api/v3/game/: 400.BadRequest"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|") && v.Contains("/api/v3/game/: 400.BadRequest"));
        }
    }
}
