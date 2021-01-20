﻿using Discord;
using Discord.Commands;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GladosV3
{
    public sealed class Program
    {
        public static void Main(string[] args)  
            =>  StartAsync(args).GetAwaiter().GetResult();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public static async Task StartAsync(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Directory.SetCurrentDirectory(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
            var pInvokeDir = Path.Combine(Directory.GetCurrentDirectory(), "PInvoke\\");
            if (!Directory.Exists(pInvokeDir))
            { Console.WriteLine("PInvoke directory doesn't exist! Creating!"); Directory.CreateDirectory(pInvokeDir); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !PInvokes_DllImport.SetDllDirectory(pInvokeDir)) Console.WriteLine($"Failed to call SetDllDirectory PInvoke! Last error code: {Marshal.GetLastWin32Error()}");
            Tools.ReleaseMemory();
            LoggingService.Begin();
            /*if(!IsValidJson())
            { await Task.Delay(10000); return; }*/
            var client =
                new DiscordSocketClient(new DiscordSocketConfig // Add the discord client to the service provider
                {
                    LogLevel           = LogSeverity.Verbose,
                    RateLimitPrecision = RateLimitPrecision.Millisecond,
                    ExclusiveBulkDelete =
                        true, // Disable firing message delete event on bulk delete event (bulk delete event will still be fired)
#if DEBUG
                    RestClientProvider = DefaultRestClientProvider.Create(true),
                    WebSocketProvider  = DefaultWebSocketProvider.Create(new WebProxy("127.0.0.1", 8888)),
#endif
                    MessageCacheSize =
                        0,                                   // Tell Discord.Net to NOT CACHE! This will also disable MessageUpdated event
                    DefaultRetryMode = RetryMode.AlwaysRetry // Always believe
                });
            var services = new ServiceCollection()      // Begin building the service provider
                .AddSingleton(client)
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
                .AddSingleton<BotSettingsHelper<string>>();
            foreach (var item in (await new ExtensionLoadingService().GetServices(client, services).ConfigureAwait(true))) 
                services.AddSingleton(item);
            var provider = services.BuildServiceProvider();     // Create the service provide
            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync(args).ConfigureAwait(false);
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IPLoggerProtection>();
            MemoryHandlerService.Start();
            await Task.Delay(-1).ConfigureAwait(true);     // Prevent the application from closing
        }
    }
}
