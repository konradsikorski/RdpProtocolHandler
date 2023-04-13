using NLog;
using System;
using System.Diagnostics;

namespace KonradSikorski.Tools.RdpProtocolHandler
{
    internal class RdpHandler
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static void Run(string parameter)
        {
            Log.Debug("Start RDP: " + parameter);

            var rdpAppPath = GetPathToRemoteDesktopApp();
            var rdpParametersChain = BuildParameters(parameter);

            Process.Start(rdpAppPath, rdpParametersChain);
            Log.Debug("End RDP");
        }

        private static string GetPathToRemoteDesktopApp()
        {
            return $"{Environment.GetEnvironmentVariable("systemroot")}\\system32\\mstsc.exe";
        }

        private static string BuildParameters(string parameter)
        {
            var uri = parameter.Substring("rdp://".Length).TrimEnd('/');
            var rdpParameters = uri.Split(',');

            rdpParameters[0] = $"/v:{rdpParameters[0]}";
            for (int i = 1; i < rdpParameters.Length; i++)
            {
                var rdpParam = rdpParameters[i];
                if (!string.IsNullOrWhiteSpace(rdpParam)) rdpParameters[i] = "/" + rdpParam;
            }

            var rdpParametersChain = string.Join(" ", rdpParameters);
            Log.Debug("rdpParametersChain: " + rdpParametersChain);
            return rdpParametersChain;
        }

    }
}
