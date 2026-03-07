using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DryIoc;
using NLog;
using Playarr.Common.Composition.Extensions;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation;
using Playarr.Common.Instrumentation.Extensions;
using Playarr.Common.Processes;
using Playarr.Update.UpdateEngine;

namespace Playarr.Update
{
    public class UpdateApp
    {
        private readonly IInstallUpdateService _installUpdateService;
        private readonly IProcessProvider _processProvider;

        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(UpdateApp));

        public UpdateApp(IInstallUpdateService installUpdateService, IProcessProvider processProvider)
        {
            _installUpdateService = installUpdateService;
            _processProvider = processProvider;
        }

        public static void Main(string[] args)
        {
            try
            {
                var startupArgument = new StartupContext(args);
                PlayarrLogger.Register(startupArgument, true, true);

                Logger.Info("Starting Playarr Update Client");

                var container = new Container(rules => rules.WithPlayarrRules())
                    .AutoAddServices(new List<string> { "Playarr.Update" })
                    .AddPlayarrLogger()
                    .AddStartupContext(startupArgument);

                container.Resolve<InitializeLogger>().Initialize();
                container.Resolve<UpdateApp>().Start(args);

                Logger.Info("Update completed successfully");
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "An error has occurred while applying update package.");
            }
        }

        public void Start(string[] args)
        {
            var startupContext = ParseArgs(args);
            var targetFolder = GetInstallationDirectory(startupContext);

            _installUpdateService.Start(targetFolder, startupContext.ProcessId);
        }

        private UpdateStartupContext ParseArgs(string[] args)
        {
            if (args == null || !args.Any())
            {
                throw new ArgumentOutOfRangeException("args", "args must be specified");
            }

            var startupContext = new UpdateStartupContext
            {
                ProcessId = ParseProcessId(args[0])
            };

            if (OsInfo.IsNotWindows)
            {
                switch (args.Length)
                {
                    case 1:
                        return startupContext;
                    default:
                        {
                            Logger.Debug("Arguments:");

                            foreach (var arg in args)
                            {
                                Logger.Debug("  {0}", arg);
                            }

                            startupContext.UpdateLocation = args[1];
                            startupContext.ExecutingApplication = args[2];

                            break;
                        }
                }
            }

            return startupContext;
        }

        private int ParseProcessId(string arg)
        {
            if (!int.TryParse(arg, out var id) || id <= 0)
            {
                throw new ArgumentOutOfRangeException("arg", "Invalid process ID");
            }

            Logger.Debug("Playarr process ID: {0}", id);
            return id;
        }

        private string GetInstallationDirectory(UpdateStartupContext startupContext)
        {
            if (startupContext.ExecutingApplication.IsNullOrWhiteSpace())
            {
                Logger.Debug("Using process ID to find installation directory: {0}", startupContext.ProcessId);
                var exeFileInfo = new FileInfo(_processProvider.GetProcessById(startupContext.ProcessId).StartPath);
                Logger.Debug("Executable location: {0}", exeFileInfo.FullName);

                return exeFileInfo.DirectoryName;
            }
            else
            {
                Logger.Debug("Using executing application: {0}", startupContext.ExecutingApplication);
                var exeFileInfo = new FileInfo(startupContext.ExecutingApplication);
                Logger.Debug("Executable location: {0}", exeFileInfo.FullName);

                return exeFileInfo.DirectoryName;
            }
        }
    }
}
