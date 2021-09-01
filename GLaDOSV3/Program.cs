using Discord;
using Discord.Commands;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using GLaDOSV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using GLaDOSV3.Dashboard;
using GLaDOSV3.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GLaDOSV3
{
    //TODO: use https://github.com/Quahu/Qmmands
    //TODO: Make timeout attribute better
    public static class Program
    {

        public static void Main(string[] args)
            => StartAsync(args).GetAwaiter().GetResult();
        public static async Task StartAsync(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                        .WriteTo.Console()
                        .WriteTo.File("Logs/main-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                        .CreateLogger();
            if (StaticTools.IsWindows() && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                Log.Fatal("This program does not support Windows 7. You have been warned...");
            //DashboardClient.Connect();
            ConsoleHelper.EnableVirtualConsole();
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.White;

            Debug.Assert(Path.GetDirectoryName(AppContext.BaseDirectory) != null, "Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException()");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException());
            var pInvokeDir = Path.Combine(Directory.GetCurrentDirectory(), $"PInvoke{Path.DirectorySeparatorChar}");
            if (!Directory.Exists(pInvokeDir))
            { Console.WriteLine("PInvoke directory doesn't exist! Creating!"); Directory.CreateDirectory(pInvokeDir); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !PInvokesDllImport.SetDllDirectory(pInvokeDir)) Console.WriteLine($"Failed to call SetDllDirectory PInvoke! Last error code: {Marshal.GetLastWin32Error()}");
            Tools.ReleaseMemory();
            LoggingService.Begin();
            DiscordShardedClient client =
                new DiscordShardedClient(new DiscordSocketConfig // Add the discord client to the service provider
                {
                    LogLevel = LogSeverity.Verbose,
#if false
                    RestClientProvider = DefaultRestClientProvider.Create(true),
                    WebSocketProvider  = DefaultWebSocketProvider.Create(new System.Net.WebProxy("127.0.0.1", 8888)),
#endif
                    MessageCacheSize =
                        0,                                   // Tell Discord.Net to NOT CACHE! This will also disable MessageUpdated event
                    DefaultRetryMode = RetryMode.AlwaysRetry, // Always believe
                    GatewayIntents = GatewayIntents.All
                });
            HostBuilder hostBuilder = new HostBuilder();
            hostBuilder.UseSerilog();
            hostBuilder.UseConsoleLifetime();
            hostBuilder.UseContentRoot(Path.GetDirectoryName(AppContext.BaseDirectory));
            ExtensionLoadingService.Init(null, null, null, null);
            ExtensionLoadingService.LoadExtensions();
            hostBuilder.GladosServices(client);
            var host = hostBuilder.Build();
            var provider = host.Services;
            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync(args).ConfigureAwait(false);
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IpLoggerProtection>();
            try
            {
                await Task.Delay(-1); // Prevent the application from closing
            } finally{ Log.CloseAndFlush(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private static void GladosServices(this HostBuilder builder, DiscordShardedClient client)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            builder.ConfigureServices(async (_, services) =>
            {
                services.AddHostedService<MemoryHandlerService>();
                services.AddSingleton(client)
                        .AddSingleton(new CommandService(new
                                                             CommandServiceConfig // Add the command service to the service provider
                        {
                            DefaultRunMode =
                                                                     RunMode.Async, // Force all commands to run async
                            LogLevel = LogSeverity.Verbose,
                            SeparatorChar = ' ', // Arguments
                            ThrowOnError = true // This could be changed to false
                        }))
                        .AddSingleton<CommandHandler>()     // Add remaining services to the provider
                        .AddSingleton<LoggingService>()     // Bad idea not logging commands 
                        .AddSingleton<StartupService>()     // Do commands on startup
                        .AddSingleton<OnLogonService>()     // Execute commands after websocket connects
                        .AddSingleton<ClientEvents>()       // Discord client events
                        .AddSingleton<IpLoggerProtection>() // IP logging service
                        .AddSingleton<BotSettingsHelper<string>>();

                foreach (Type[] item in ExtensionLoadingService.GetServices(client, services))
                    foreach (Type t in item) { services.AddSingleton(t); }
            });
        }
    }
}
