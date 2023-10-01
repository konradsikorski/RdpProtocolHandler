using Microsoft.Win32;
using NLog;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace KonradSikorski.Tools.RdpProtocolHandler
{
    [SupportedOSPlatform("windows")]
    internal static class Installer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string REGISTRY_KEY_NAME = "RDP";

        internal static void Uninstall()
        {
            ConsoleWrapper.Alloc();
            if (!RequireAdministratorPrivileges()) return;

            Registry.ClassesRoot.DeleteSubKeyTree(REGISTRY_KEY_NAME, false);
            ConsoleWrapper.WriteLine("RDP Protocol Handler uninstalled.");
            Log.Info("RDP Protocol Handler uninstalled.");
        }

        [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
        internal static void Install()
        {
            ConsoleWrapper.Alloc();

            if (!RequireAdministratorPrivileges()) return;

            Uninstall();

            //-- get assembly info
            var handlerLocation = GetAppPath();

            //-- create registry structure
            var rootKey = Registry.ClassesRoot.CreateSubKey(REGISTRY_KEY_NAME);
            var defaultIconKey = rootKey?.CreateSubKey("DefaultIcon");
            var commandKey = rootKey?.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command");

            rootKey?.SetValue("", "rdp:Remote Desktop Protocol");
            rootKey?.SetValue("URL Protocol", "");
            defaultIconKey?.SetValue("", @"C:\Windows\System32\mstsc.exe");
            commandKey?.SetValue("", $@"""{handlerLocation}"" ""%1""");

            //--
            Log.Info("RDP Protocol Handler installed");
            ConsoleWrapper.WriteLine("RDP Protocol Handler installed");
            ConsoleWrapper.WriteLine($"WARNING: Do not move this file '{handlerLocation}' to other location, otherwise handler will not work. If you change the location run installation process again.");
        }

        private static string GetAppPath()
        {
            // Get filename
            string fileName = Process.GetCurrentProcess().MainModule.FileName;

            // If published as single file, the fileName will be the temp path to the actual binary
            // You can use Path.GetFileName to get the executable name and combine it with AppContext.BaseDirectory
            if (fileName.StartsWith(Path.GetTempPath()))
            {
                var directory = AppContext.BaseDirectory;
                fileName = Path.Combine(directory, Path.GetFileName(fileName));
            }

            return fileName;
        }

        private static bool RequireAdministratorPrivileges()
        {
            var isAdmin = IsUserAdministrator();

            if (!isAdmin)
            {
                var oldColor = ConsoleWrapper.ForegroundColor;
                ConsoleWrapper.ForegroundColor = ConsoleColor.Red;
                ConsoleWrapper.WriteLine("You must be system administrator");
                ConsoleWrapper.ForegroundColor = oldColor;
                Log.Error("You must be system administrator");
            }

            return isAdmin;
        }

        private static bool IsUserAdministrator()
        {
            using (var user = WindowsIdentity.GetCurrent())
            {
                try
                {
                    //get the currently logged in user
                    var principal = new WindowsPrincipal(user);
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
