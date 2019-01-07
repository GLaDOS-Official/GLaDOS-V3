using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GladosV3
{
    public class Program
    {
        public static void Main()
            => new Program().StartAsync().GetAwaiter().GetResult();

        private IConfigurationRoot _config;

        public async Task StartAsync()
        {
            var pInvokeDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "PInvoke\\");
            if (!Directory.Exists(pInvokeDir))
            { Console.WriteLine("PInvoke directory doesn't exist! Creating!"); Directory.CreateDirectory(pInvokeDir); }
            if(!PInvokes_DllImport.SetDllDirectory(pInvokeDir))
                Console.WriteLine($"Failed to call SetDllDirectory PInvoke! Last error code: {System.Runtime.InteropServices.Marshal.GetLastWin32Error()}");
            Tools.ReleaseMemory();
            LoggingService.Begin();
            if(!IsValidJson())
            { await Task.Delay(10000); return; }
            _config = await Tools.GetConfigAsync();              // Build the configuration file

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
                .AddSingleton(_config);
            foreach(var item in new ExtensionLoadingService().GetServices().GetAwaiter().GetResult())
            {
                services.AddSingleton(item);
            }
            var provider = services.BuildServiceProvider();     // Create the service provider

            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, client events, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<ClientEvents>();
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<IPLoggerProtection>();
            MemoryHandlerService.Start();
            await Task.Delay(-1);     // Prevent the application from closing
        }
        internal bool IsValidJson()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "_configuration.json");
            if (!File.Exists(path))
            { LoggingService.Log(LogSeverity.Error, "GLaDOS V3", "_configuration.json file does not exist!").GetAwaiter(); return false; }
            try
            {
                JToken.Parse(File.ReadAllTextAsync(path).GetAwaiter().GetResult());
            }
            catch(JsonReaderException e)
            { LoggingService.Log(LogSeverity.Error, "GLaDOS V3", $"_configuration.json file is not a valid json file! {e.Message}").GetAwaiter(); return false; }
            return true;
        }
    }
}
