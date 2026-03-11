using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using Playarr.Common.Instrumentation;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Instrumentation;
using Playarr.Core.MediaFiles;
using Playarr.Core.Test.Framework;
using Playarr.Test.Common;

namespace Playarr.Core.Test.InstrumentationTests
{
    [TestFixture]
    public class DatabaseTargetFixture : DbTest<DatabaseTarget, Log>
    {
        private static string _uniqueMessage;
        private Logger _logger;

        protected override MigrationType MigrationType => MigrationType.Log;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<ILogRepository, LogRepository>();
            Mocker.Resolve<DatabaseTarget>().Register();

            LogManager.ReconfigExistingLoggers();

            _logger = PlayarrLogger.GetLogger(this);

            _uniqueMessage = "Unique message: " + Guid.NewGuid();
        }

        [Test]
        public void write_log()
        {
            _logger.Info(_uniqueMessage);

            Thread.Sleep(1000);

            var logItem = AllStoredModels.First(l => l.Message == _uniqueMessage);
            logItem.Message.Should().Be(_uniqueMessage);
            VerifyLog(logItem, LogLevel.Info);
        }

        [Test]
        public void write_long_log()
        {
            var message = string.Empty;
            for (var i = 0; i < 100; i++)
            {
                message += Guid.NewGuid();
            }

            _logger.Info(message);

            Thread.Sleep(1000);

            var logItem = AllStoredModels.First(l => l.Message == message);
            logItem.Message.Should().HaveLength(message.Length);
            logItem.Message.Should().Be(message);
            VerifyLog(logItem, LogLevel.Info);
        }

        [Test]
        public void write_log_exception()
        {
            var ex = new InvalidOperationException("Fake Exception");

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(1000);

            var logItem = AllStoredModels.First(l => l.Message.Contains(_uniqueMessage));
            VerifyLog(logItem, LogLevel.Error);
            logItem.Message.Should().Be(_uniqueMessage + ": " + ex.Message);
            logItem.ExceptionType.Should().Be(ex.GetType().ToString());
            logItem.Exception.Should().Be(ex.ToString());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void exception_log_with_no_message_should_use_exceptions_message()
        {
            var ex = new InvalidOperationException("Fake Exception");
            _uniqueMessage = string.Empty;

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(1000);

            var logItem = AllStoredModels.First(l => l.Message == ex.Message);
            logItem.Message.Should().Be(ex.Message);

            VerifyLog(logItem, LogLevel.Error);

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            var epFile = new RomFile();
            _logger.Debug("File {0} no longer exists on disk. removing from database.", epFile.RelativePath);

            Thread.Sleep(1000);

            epFile.RelativePath.Should().BeNull();
        }

        [TearDown]
        public void Teardown()
        {
            Mocker.Resolve<DatabaseTarget>().UnRegister();
        }

        private void VerifyLog(Log logItem, LogLevel level)
        {
            logItem.Time.Should().BeWithin(TimeSpan.FromSeconds(2));
            logItem.Logger.Should().Be(GetType().Name);
            logItem.Level.Should().Be(level.Name);
            _logger.Name.Should().EndWith(logItem.Logger);
        }
    }
}
