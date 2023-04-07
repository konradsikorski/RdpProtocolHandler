using NLog;
using System;
using System.Diagnostics;

namespace KonradSikorski.Tools.RdpProtocolHandler;

class Program
{
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        LogHelper.ConfigureNLog();
        Log.Info($"{string.Join(" | ", args)}");

        if (args.Length == 0)
        {
            InstallationHelper.Install();
        }
        else
        {
            var parameter = args[0];

            switch (parameter.ToLower())
            {
                case "/uninstall": InstallationHelper.Uninstall(); break;
                case "/install": InstallationHelper.Install(false); break;
                case "/log": LogHelper.OpenLogFile(); break;
                case "/help":
                case "/?":
                    InstallationHelper.Help(); 
                    break;
                default:
                    Rdp(parameter);
                    break;
            }
        }

        ConsoleWrapper.WaitForClose();
    }

    private static void Rdp(string parameter)
    {
        Log.Debug("Start RDP: " + parameter);

        var uri = parameter.Substring("rdp://".Length).TrimEnd('/');
        var rdpParameters = uri.Split(',');

        rdpParameters[0] = $"/v:{rdpParameters[0]}";
        for (var i = 1; i < rdpParameters.Length; i++)
        {
            var rdpParam = rdpParameters[i];
            if (!string.IsNullOrWhiteSpace(rdpParam)) rdpParameters[i] = "/" + rdpParam;
        }

        var rdpParametersChain = string.Join(" ", rdpParameters);
        Log.Debug("rdpParametersChain: " + rdpParametersChain);

        Process.Start($"{Environment.GetEnvironmentVariable("systemroot")}\\system32\\mstsc.exe", rdpParametersChain);
        Log.Debug("End RDP");
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ConsoleWrapper.Alloc();
        Log.Error(e.ExceptionObject);
        ConsoleWrapper.WriteLine("Error occurred. Please check the log file for details.");
        Environment.Exit(1);
    }
}
