using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GladosV3
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync(args).GetAwaiter().GetResult();


        public async Task StartAsync(string[] args)
        {
            Console.ResetColor();
            Directory.SetCurrentDirectory(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
            var PInvokeDir = Path.Combine(Directory.GetCurrentDirectory(), "PInvoke\\");
            if (!Directory.Exists(PInvokeDir))
            { Console.WriteLine("PInvoke directory doesn't exist! Creating!"); Directory.CreateDirectory(PInvokeDir); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !PInvokes_DllImport.SetDllDirectory(PInvokeDir)) Console.WriteLine($"Failed to call SetDllDirectory PInvoke! Last error code: {System.Runtime.InteropServices.Marshal.GetLastWin32Error()}");
            Tools.ReleaseMemory();
            LoggingService.Begin();
            /*if(!IsValidJson())
            { await Task.Delay(10000); return; }*/

            var services = new ServiceCollection()      // Begin building the service provider
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig     // Add the discord client to the service provider
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 0,    // Tell Discord.Net to NOT CACHE! This will also disable MessageUpdated event
                    DefaultRetryMode = RetryMode.AlwaysRetry // Always believe
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig     // Add the command service to the service provider
                {
                    DefaultRunMode = RunMode.Async,     // Force all commands to run async
                    LogLevel = LogSeverity.Verbose,
                    SeparatorChar = ' ',  // Arguments
                    ThrowOnError = true // This could be changed to false
                }))
                .AddSingleton<CommandHandler>()     // Add remaining services to the provider
                .AddSingleton<LoggingService>()     // Bad idea not logging commands 
                .AddSingleton<StartupService>()     // Do commands on startup
                .AddSingleton<OnLogonService>()     // Execute commands after websocket connects
                .AddSingleton<ClientEvents>()       // Discord client events
                .AddSingleton<IPLoggerProtection>()       // IP logging service
                .AddSingleton<BotSettingsHelper<string>>()
                ;//.AddSingleton(_config);
            foreach (var item in new ExtensionLoadingService().GetServices().GetAwaiter().GetResult())
            {
                services.AddSingleton(item);
            }
            var provider = services.BuildServiceProvider();     // Create the service provide

            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync(args);
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IPLoggerProtection>();
            MemoryHandlerService.Start();
            await Task.Delay(-1);     // Prevent the application from closing
        }
    }
}
