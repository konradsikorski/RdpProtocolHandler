using System;
using System.Runtime.Versioning;
using NLog;

namespace KonradSikorski.Tools.RdpProtocolHandler
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            LoggerHelper.ConfigureNLog();
            Log.Info($"{string.Join( " | ", args)}");

            if (args.Length == 0) Installer.Install();
            else
            {
                var parameter = args[0];
                switch (parameter.ToLower())
                {
                    case "/uninstall": Installer.Uninstall(); break;
                    case "/install": Installer.Install(false); break;
                    case "/log": LoggerHelper.OpenLogFile(); break;
                    case "/help":
                    case "/?":
                        Help();
                        break;
                    default:
                        RdpHandler.Run(parameter);
                        break;
                }
            }

            ConsoleWrapper.WaitForClose();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ConsoleWrapper.Alloc();
            Log.Error(e.ExceptionObject);
            ConsoleWrapper.WriteLine("Error occurred. Please check the log file for details");
            Environment.Exit(1);
        }

        private static void Help()
        {
            ConsoleWrapper.Alloc();
            ConsoleWrapper.WriteLine("For help go to: https://github.com/konradsikorski/RdpProtocolHandler");
        }
    }
}
