using Microsoft.Win32;
using NLog.Fluent;
using System.Reflection;
using System.Security.Principal;
using System;

namespace KonradSikorski.Tools.RdpProtocolHandler;

internal static class InstallationHelper
{
    private const string REGISTRY_KEY_NAME = "RDP";

    internal static void Help()
    {
        ConsoleWrapper.Alloc();
        ConsoleWrapper.WriteLine("For help go to: https://github.com/LanceMcCarthy/RdpProtocolHandler");
    }

    internal static void Uninstall()
    {
        ConsoleWrapper.Alloc();
        if (!RequireAdministratorPrivileges()) return;

        Registry.ClassesRoot.DeleteSubKeyTree(REGISTRY_KEY_NAME, false);
        ConsoleWrapper.WriteLine("RDP Protocol Handler uninstalled.");
        Log.Info("RDP Protocol Handler uninstalled.");
    }

    internal static void Install(bool prompt = true)
    {
        ConsoleWrapper.Alloc();

        if (!RequireAdministratorPrivileges()) return;

        //if (prompt)
        //{
        //    ConsoleWrapper.Write("Do you want to install RDP Protocol handler? (for details use /?) [Y]es [N]o:");
        //    var result = ConsoleWrapper.ReadLine();
        //    if (result?.ToLower() != "y") return;
        //}

        Uninstall();

        //-- get assembly info
        var assembly = Assembly.GetExecutingAssembly();
        var handlerLocation = assembly.Location;

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
        ConsoleWrapper.WriteLine($"WARNING: Do not move this '{assembly.FullName}' to other location, otherwise handler will not work. If you change the location run installation process again.");
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
        using var user = WindowsIdentity.GetCurrent();

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