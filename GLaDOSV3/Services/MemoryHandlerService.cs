using GLaDOSV3.Helpers;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace GLaDOSV3.Services
{
    public class MemoryHandlerService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Releasing unused memory....");
                Tools.ReleaseMemory();
                ConsoleHelper.WriteColorLine(ConsoleColor.Cyan, "[MemoryHandlerThread] Memory released, another recycle in 30 minutes!");
                Thread.Sleep(1800000);
            }
            return Task.CompletedTask;
        }
    }
}
