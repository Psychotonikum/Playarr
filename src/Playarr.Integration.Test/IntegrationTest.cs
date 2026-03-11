using System;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Datastore;
using Playarr.Core.Datastore.Migration.Framework;
using Playarr.Core.Indexers.Newznab;
using Playarr.Test.Common;
using Playarr.Test.Common.Datastore;

namespace Playarr.Integration.Test
{
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class IntegrationTest : IntegrationTestBase
    {
        protected static int StaticPort = 9797;

        protected PlayarrRunner _runner;

        public override string SeriesRootFolder => GetTempDirectory("SeriesRootFolder");

        protected int Port { get; private set; }

        protected PostgresOptions PostgresOptions { get; set; } = new();

        protected override string RootUrl => $"http://localhost:{Port}/";

        protected override string ApiKey => _runner.ApiKey;

        protected override void StartTestTarget()
        {
            Port = Interlocked.Increment(ref StaticPort);

            PostgresOptions = PostgresDatabase.GetTestOptions();

            if (PostgresOptions?.Host != null)
            {
                CreatePostgresDb(PostgresOptions);
            }

            _runner = new PlayarrRunner(LogManager.GetCurrentClassLogger(), PostgresOptions, Port);
            _runner.Kill();

            _runner.Start();
        }

        protected override void InitializeTestTarget()
        {
            // Make sure tasks have been initialized so the config put below doesn't cause errors
            WaitForCompletion(() => Tasks.All().SelectList(x => x.TaskName).Contains("RssSync"));

            var indexer = Indexers.Schema().FirstOrDefault(i => i.Implementation == nameof(Newznab));

            if (indexer == null)
            {
                throw new NullReferenceException("Expected valid indexer schema, found null");
            }

            indexer.EnableRss = false;
            indexer.EnableInteractiveSearch = false;
            indexer.EnableAutomaticSearch = false;
            indexer.ConfigContract = nameof(NewznabSettings);
            indexer.Implementation = nameof(Newznab);
            indexer.Name = "NewznabTest";
            indexer.Protocol = Core.Indexers.DownloadProtocol.Usenet;

            // Change Console Log Level to Debug so we get more details.
            var config = HostConfig.Get(1);
            config.ConsoleLogLevel = "Debug";
            HostConfig.Put(config);

            // Configure IGDB/Twitch credentials from environment or .env.local
            var twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID") ?? string.Empty;
            var twitchClientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(twitchClientId) || string.IsNullOrWhiteSpace(twitchClientSecret))
            {
                // Try loading from .env.local file in workspace root
                var envFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", ".env.local");

                if (File.Exists(envFile))
                {
                    foreach (var line in File.ReadAllLines(envFile))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                        {
                            continue;
                        }

                        var eqIndex = trimmed.IndexOf('=');
                        if (eqIndex > 0)
                        {
                            var key = trimmed.Substring(0, eqIndex).Trim();
                            var value = trimmed.Substring(eqIndex + 1).Trim();

                            if (key == "TWITCH_CLIENT_ID")
                            {
                                twitchClientId = value;
                            }
                            else if (key == "TWITCH_CLIENT_SECRET")
                            {
                                twitchClientSecret = value;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(twitchClientId) && !string.IsNullOrWhiteSpace(twitchClientSecret))
            {
                var metadataConfig = MetadataSourceConfig.Get(1);
                metadataConfig.TwitchClientId = twitchClientId;
                metadataConfig.TwitchClientSecret = twitchClientSecret;
                MetadataSourceConfig.Put(metadataConfig);
            }
        }

        protected override void StopTestTarget()
        {
            _runner.Kill();
            if (PostgresOptions?.Host != null)
            {
                DropPostgresDb(PostgresOptions);
            }
        }

        private static void CreatePostgresDb(PostgresOptions options)
        {
            PostgresDatabase.Create(options, MigrationType.Main);
            PostgresDatabase.Create(options, MigrationType.Log);
        }

        private static void DropPostgresDb(PostgresOptions options)
        {
            PostgresDatabase.Drop(options, MigrationType.Main);
            PostgresDatabase.Drop(options, MigrationType.Log);
        }
    }
}
