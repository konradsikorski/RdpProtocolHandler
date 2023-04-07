using NLog.Config;
using NLog.Targets;
using NLog;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq;

namespace KonradSikorski.Tools.RdpProtocolHandler;

internal static class LogHelper
{
    internal static void ConfigureNLog()
    {
        if (LogManager.Configuration != null) return;

        var logConfiguration = new LoggingConfiguration();
        var fileTarget = new FileTarget("file")
        {
            Layout = "${longdate} ${uppercase:${level}} ${message}",
            FileName = Path.Combine(Path.GetTempPath(), @"rdppotocolhandler-logs\${shortdate}.log")
        };

        var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
        logConfiguration.LoggingRules.Add(rule);

        LogManager.Configuration = logConfiguration;
    }

    internal static void OpenLogFile()
    {
        var fileTarget = LogManager.Configuration.AllTargets.OfType<FileTarget>().FirstOrDefault();

        if (fileTarget == null) return;

        var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
        var fileName = fileTarget.FileName.Render(logEventInfo);
        if (File.Exists(fileName))
            Process.Start(fileName);
    }
}