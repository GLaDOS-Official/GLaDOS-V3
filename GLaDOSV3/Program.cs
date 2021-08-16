using Discord;
using Discord.Commands;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using GLaDOSV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using GLaDOSV3.Dashboard;
using GLaDOSV3.Models;
using Microsoft.Extensions.Hosting;

namespace GLaDOSV3
{      
    //TODO: use https://github.com/Quahu/Qmmands
    //TODO: Make timeout attribute better
    public static class Program
    {

        public static void Main(string[] args)
            => StartAsync(args).GetAwaiter().GetResult();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public static async Task StartAsync(string[] args)
        {
            //DashboardClient.Connect();
            ConsoleHelper.EnableVirtualConsole();
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.White;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory));
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
            ExtensionLoadingService.Init(null, null, null, null);
            ExtensionLoadingService.LoadExtensions();
            HostBuilder hostBuilder = new HostBuilder();
            hostBuilder.UseContentRoot(Path.GetDirectoryName(AppContext.BaseDirectory));
            hostBuilder.gladosServices(client);
            var host = hostBuilder.Build();

            var provider = host.Services;
            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync(args).ConfigureAwait(false);
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IpLoggerProtection>();

            await Task.Delay(-1).ConfigureAwait(true);     // Prevent the application from closing
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        private static void gladosServices(this HostBuilder builder, DiscordShardedClient client)
        {
            builder.ConfigureServices(async (hostContext, services) =>
            {
                services.AddHostedService<MemoryHandlerService>();
                services.AddSingleton(client)
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
                .AddSingleton<IpLoggerProtection>()       // IP logging service
                .AddSingleton<BotSettingsHelper<string>>();
                foreach (var item in (await ExtensionLoadingService.GetServices(client, services).ConfigureAwait(true)))
                    services.AddSingleton(item);
            });
        }
    }
}
