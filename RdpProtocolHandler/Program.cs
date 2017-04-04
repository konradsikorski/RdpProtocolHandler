using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Microsoft.Win32;
using NLog;
using NLog.Targets;

namespace KonradSikorski.Tools.RdpProtocolHandler
{
    class Program
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string REGISTRY_KEY_NAME = "RDP";

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Log.Info($"{string.Join( " | ", args)}");

            if (args.Length == 0) Install();
            else
            {
                var parameter = args[0];
                switch (parameter.ToLower())
                {
                    case "/uninstall": Uninstall(); break;
                    case "/install": Install(false); break;
                    case "/log": OpenLogFile(); break;
                    case "/help":
                    case "/?":
                        Help();
                        break;
                    default:
                        Rdp(parameter);
                        break;
                }
            }
        }

        private static void OpenLogFile()
        {
            var fileTarget = LogManager.Configuration.AllTargets.OfType<FileTarget>().FirstOrDefault();

            if (fileTarget == null) return;

            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            string fileName = fileTarget.FileName.Render(logEventInfo);
            if (File.Exists(fileName))
                Process.Start(fileName);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject);
            Console.WriteLine("Error occured. Please check the log file for details");
            Environment.Exit(1);
        }

        private static void Help()
        {
            throw new NotImplementedException();
        }

        private static void Rdp(string parameter)
        {
            Log.Debug("Start RDP: " + parameter);

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

            Process.Start($"{Environment.GetEnvironmentVariable("systemroot")}\\system32\\mstsc.exe", rdpParametersChain);
            Log.Debug("End RDP");
        }

        private static void Uninstall()
        {
            if (!RequireAdministratorPrivilages()) return;

            Registry.ClassesRoot.DeleteSubKeyTree(REGISTRY_KEY_NAME, false);
            Console.WriteLine("RDP Protocol Handler uninstalled.");
            Log.Info("RDP Protocol Handler uninstalled."); 
        }

        private static void Install(bool prompt = true)
        {
            if (!RequireAdministratorPrivilages()) return;

            //if (prompt)
            //{
            //    Console.Write("Do you want to install RDP Protocol handler? (for details use /?) [Y]es [N]o:");
            //    var result = Console.ReadLine();
            //    if (result?.ToLower() != "y") return;
            //}

            Uninstall();
            
            //-- get assembly info
            var assembly = Assembly.GetExecutingAssembly();
            var handlerLocation = assembly.Location;

            //-- create registy structure
            var rootKey = Registry.ClassesRoot.CreateSubKey(REGISTRY_KEY_NAME);
            var defaultIconKey = rootKey?.CreateSubKey("DefaultIcon");
            var commandKey = rootKey?.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command");

            rootKey?.SetValue("", "rdp:Remote Desktop Protocol" );
            rootKey?.SetValue("URL Protocol", "");
            defaultIconKey?.SetValue("", @"C:\Windows\System32\mstsc.exe");
            commandKey?.SetValue("", $@"""{handlerLocation}"" ""%1""");

            //--
            Log.Info("RDP Protocol Handler installed");
            Console.WriteLine("RDP Protocol Handler installed");
            Console.WriteLine($"WARNING: Do not move this '{assembly.FullName}' to other location, otherwise handler will not work. If you change the location run installation process again.");
        }

        private static bool RequireAdministratorPrivilages()
        {
            var isAdmin = IsUserAdministrator();

            if (!isAdmin)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You must be system administrator");
                Console.ForegroundColor = oldColor;
                Log.Error("You must be system administrator");
            }

            return isAdmin;
        }

        private static bool IsUserAdministrator()
        {
            using (WindowsIdentity user = WindowsIdentity.GetCurrent())
            {
                try
                {
                    //get the currently logged in user
                    WindowsPrincipal principal = new WindowsPrincipal(user);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }
        }
    }
}
