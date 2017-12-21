using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using GladosV3.Services;
using GladosV3.Helpers;
namespace GladosV3
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private IConfigurationRoot _config;

        public async Task StartAsync()
        {
            _config = await Tools.GetConfig();              // Build the configuration file

            var services = new ServiceCollection()      // Begin building the service provider
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig     // Add the discord client to the service provider
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,    // Tell Discord.Net to cache 1000 messages per channel
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
                .AddSingleton<Random>()             // You get better random with a single instance than by creating a new one every time you need it
                .AddSingleton<SystemMessage>()      // System message, simple
                .AddSingleton<IsOwner>()            // I don't like the way Discord.NET handles owner attribute
                .AddSingleton<OnLogonService>()     // Execute commands after websocket connects
                .AddSingleton(_config);

            var provider = services.BuildServiceProvider();     // Create the service provider

            provider.GetRequiredService<LoggingService>();      // Initialize the logging service, startup service, on discord log on service, command handler and system message
            provider.GetRequiredService<OnLogonService>();
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<SystemMessage>().KeyPress();
            await Task.Delay(-1);     // Prevent the application from closing
        }
    }
}
