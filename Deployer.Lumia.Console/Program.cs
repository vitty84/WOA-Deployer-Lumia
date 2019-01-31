﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Deployer.Lumia.NetFx;
using Deployment.Console.Options;
using Serilog;
using Serilog.Events;

namespace Deployment.Console
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            ConfigureLogger();

            try
            {
                await Parser.Default
                    .ParseArguments<WindowsDeploymentCmdOptions, EnableDualBootCmdOptions, DisableDualBootCmdOptions,
                        InstallGpuCmdOptions>(args)
                    .MapResult(
                        (WindowsDeploymentCmdOptions opts) => ConsoleDeployer.ExecuteWindowsScript(opts),
                        (EnableDualBootCmdOptions opts) => new AdditionalActions().ToogleDualBoot(true),
                        (DisableDualBootCmdOptions opts) => new AdditionalActions().ToogleDualBoot(false),
                        (InstallGpuCmdOptions opts) => InstallGpu(),
                        HandleErrors);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Operation failed");
                throw;
            }
        }

        private static async Task InstallGpu()
        {
            await new AdditionalActions().InstallGpu();
            System.Console.WriteLine(Resources.InstallGpuManualStep);
        }

        private static Task HandleErrors(IEnumerable<Error> errs)
        {
            System.Console.WriteLine($"Invalid command line: {string.Join("\n", errs.Select(x => x.Tag))}");
            return Task.CompletedTask;
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.RollingFile(@"Logs\{Date}.txt")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }
    }    
}