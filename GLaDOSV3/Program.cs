using Discord;
using Discord.Commands;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using GLaDOSV3.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GLaDOSV3
{
    //TODO: use https://github.com/Quahu/Qmmands
    //TODO: Make timeout attribute better
    public static class Program
    {
        private static Serilog.ILogger _logger;
        public static void Main(string[] args)
            => StartAsync(args).GetAwaiter().GetResult();
        public static async Task StartAsync(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                        .WriteTo.Async(a => a.Console())
                        .WriteTo.Async(a => a.File(new ExpressionTemplate(
                                                                          "[{@t:HH:mm:ss} {@l:u4}] {Coalesce(SourceContext, '<Unknown>')} {@m}\n{@x}"), "Logs/main-.txt", rollingInterval: RollingInterval.Day,
                                                   rollOnFileSizeLimit: true))
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "GLaDOS V3")
                        .CreateLogger();
            _logger = Log.Logger.ForContext(typeof(Program));
            if (StaticTools.IsWindows() && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                _logger.Fatal("This program does not support Windows 7. You have been warned...");
            //DashboardClient.Connect();
            ConsoleHelper.EnableVirtualConsole();
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.White;

            Debug.Assert(Path.GetDirectoryName(AppContext.BaseDirectory) != null, "Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException()");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException());
            var pInvokeDir = Path.Combine(Directory.GetCurrentDirectory(), $"PInvoke{Path.DirectorySeparatorChar}");
            if (!Directory.Exists(pInvokeDir))
            { _logger.Error("PInvoke directory doesn't exist! Creating!"); Directory.CreateDirectory(pInvokeDir); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !PInvokesDllImport.SetDllDirectory(pInvokeDir)) Console.WriteLine($"Failed to call SetDllDirectory PInvoke! Last error code: {Marshal.GetLastWin32Error()}");
            Tools.ReleaseMemory();
            //LoggingService.Begin();
            WebProxy proxy = GetDebuggerProxy();
            DiscordShardedClient client =
                new DiscordShardedClient(new DiscordSocketConfig // Add the discord client to the service provider
                {
                    LogLevel = LogSeverity.Verbose,
                    RestClientProvider = DefaultRestClientProvider.Create(proxy != null),
                    WebSocketProvider = DefaultWebSocketProvider.Create(proxy),
                    MessageCacheSize =
                        0,                                   // Tell Discord.Net to NOT CACHE! This will also disable MessageUpdated event
                    DefaultRetryMode = RetryMode.AlwaysRetry, // Always believe
                    GatewayIntents = GatewayIntents.All
                });
            client.Log += Tools.LogAsync;
            HostBuilder hostBuilder = new HostBuilder();
            hostBuilder.UseConsoleLifetime();
            hostBuilder.UseContentRoot(Path.GetDirectoryName(AppContext.BaseDirectory));
            ExtensionLoadingService.Init(null, null, null, null);
            ExtensionLoadingService.LoadExtensions();
            hostBuilder.GladosServices(client);
            IHost host = hostBuilder.Build();
            IServiceProvider provider = host.Services;
            //provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync(args).ConfigureAwait(false);
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IpLoggerProtection>();
            try
            {
                await Task.Delay(-1); // Prevent the application from closing
            }
            finally { Log.CloseAndFlush(); }
        }

        private static WebProxy GetDebuggerProxy()
        {
            Uri proxyUri = WebRequest.GetSystemWebProxy().GetProxy(new Uri("http://www.discord.com"));
            if (proxyUri == null || proxyUri.Host == "discord.com") return null;
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);
                Thread.Sleep(500);
                var endPoint = new IPEndPoint(IPAddress.Parse(proxyUri.Host), proxyUri.Port);
                socket.Connect(endPoint);
            }
            catch (Exception)
            {
                return null;
            }
            return new WebProxy(proxyUri);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private static void GladosServices(this HostBuilder builder, DiscordShardedClient client)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            builder.ConfigureServices(async (_, services) =>
            {
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
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
                                                            //.AddSingleton<LoggingService>()     // Bad idea not logging commands 
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
