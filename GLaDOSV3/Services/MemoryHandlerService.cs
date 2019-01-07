﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GladosV3.Helpers;

namespace GladosV3.Services
{
    static class MemoryHandlerService
    {
        public static void Start()
        {
            Tools.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Creating thread");
            Thread thread = new Thread(new ThreadStart(MemoryThread)) {Name = "Memory releasing thread"};
            Tools.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Starting thread");
            thread.Start();
            Tools.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandler] Thread started");
        }

        private static void MemoryThread()
        {
            Tools.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Releasing unused memory....");
            Tools.ReleaseMemory();
            Tools.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Memory released, another recycle in 30 minutes!");
            Thread.Sleep(1800000);
        }
    }
}