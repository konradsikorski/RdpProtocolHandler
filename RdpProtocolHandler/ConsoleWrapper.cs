using System;
using System.Runtime.InteropServices;

namespace KonradSikorski.Tools.RdpProtocolHandler;

public static class ConsoleWrapper
{
    public static bool Initialized { get; private set; }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    public static void Alloc()
    {
        if (Initialized) return;

        AllocConsole();
        Initialized = true;
    }

    public static ConsoleColor ForegroundColor
    {
        get
        {
            Alloc();
            return Console.ForegroundColor;
        } 
        set
        {
            Alloc();
            Console.ForegroundColor = value;
        }
    }

    public static void WriteLine(string format)
    {
        Alloc();
        Console.WriteLine(format);
    }

    public static void WaitForClose()
    {
        if (!Initialized) return;

        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }
}