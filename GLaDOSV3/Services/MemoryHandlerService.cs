using GLaDOSV3.Helpers;
using System;
using System.Threading;

namespace GLaDOSV3.Services
{
    internal static class MemoryHandlerService
    {
        public static void Start()
        {
            ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Creating thread");
            Thread thread = new Thread(MemoryThread) { Name = "Memory releasing thread" };
            ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Starting thread");
            thread.Start();
            ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Thread started");
        }

        private static void MemoryThread()
        {
            ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Releasing unused memory....");
            Tools.ReleaseMemory();
            ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Memory released, another recycle in 30 minutes!");
            Thread.Sleep(1800000);
        }
    }
}
