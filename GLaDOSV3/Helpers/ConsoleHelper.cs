using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    public static class ConsoleHelper
    {
        private delegate bool GetConsoleMode(nint hConsoleHandle, out uint dwMode);
        private delegate bool SetConsoleMode(nint hConsoleHandle, uint dwMode);
        private delegate nint GetStdHandle(int nStdHandle);
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_INPUT_HANDLE = -10;
        private const int STD_ERROR_HANDLE = -12;
        private const nint INVALID_HANDLE_VALUE = -1;
        private const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        public static void EnableVirtualConsole()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            var stdHandle = PInvokes.GetFunction<GetStdHandle>(PInvokes.GetModule("kernel32.dll"), "GetStdHandle")(STD_OUTPUT_HANDLE);
            var getConsoleMode = PInvokes.GetFunction<GetConsoleMode>(PInvokes.GetModule("kernel32.dll"), "GetConsoleMode");
            getConsoleMode(stdHandle, out var mode);
            var setConsoleMode = PInvokes.GetFunction<SetConsoleMode>(PInvokes.GetModule("kernel32.dll"), "SetConsoleMode");
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            setConsoleMode(stdHandle, mode);
        }
        private static readonly object WriteLock = new object();
        public static void WriteColorLine(ConsoleColor color, string message)
        {
            lock (WriteLock)
            {
                var fcolor = Console.ForegroundColor;
                var bcolor = Console.BackgroundColor;
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Out.Write(message);
                Console.Out.WriteLine(new String(' ', Console.BufferWidth - Console.CursorLeft));
                Console.ForegroundColor = fcolor;
                Console.BackgroundColor = bcolor;
            }
        }

        public static void WriteColor(ConsoleColor color, string message)
        {
            lock (WriteLock)
            {
                var fcolor = Console.ForegroundColor;
                var bcolor = Console.BackgroundColor;
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Out.Write(message);
                Console.ForegroundColor = fcolor;
                Console.BackgroundColor = bcolor;
            }
        }
    }
}
