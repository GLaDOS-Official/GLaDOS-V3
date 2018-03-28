using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        // IServiceProvider, DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            _provider = provider;
        }


        public async Task StartAsync()
        {
            Console.Title = _config["name"];
            Console.Clear();
            Console.SetWindowSize(150, 35);
            string discordToken = _config["tokens:discord"];     // Get the discord token from the config file
            string gameTitle = _config["discord:game"]; // Get bot's game status
            if (string.IsNullOrWhiteSpace(discordToken) || string.IsNullOrEmpty(discordToken))
            { await LoggingService.Log(LogSeverity.Critical, "Bot", "Please enter your bot's token into the `_configuration.json` file found in the applications root directory."); Environment.Exit(-1); }
            else if (!string.IsNullOrWhiteSpace(discordToken) || !string.IsNullOrEmpty(discordToken))
                await _discord.SetGameAsync(gameTitle); // set bot's game status
            try
            {
                await _discord.LoginAsync(TokenType.Bot, discordToken, true); // Login to discord
                await _discord.StartAsync(); // Connect to the websocket
            }
            catch (HttpException ex) // Some error checking
            {
                if (ex.DiscordCode == 401 || ex.HttpCode == HttpStatusCode.Unauthorized)
                    Tools.WriteColorLine(ConsoleColor.Red, "Wrong or invalid token.");
                else if (ex.DiscordCode == 502 || ex.HttpCode == HttpStatusCode.BadGateway)
                    Tools.WriteColorLine(ConsoleColor.Yellow, "Gateway unavailable.");
                else if (ex.DiscordCode == 400 || ex.HttpCode == HttpStatusCode.BadRequest)
                    Tools.WriteColorLine(ConsoleColor.Red, "Bad request. Please wait for an update.");
                Tools.WriteColorLine(ConsoleColor.Red,
                    $"Discord has returned an error code: {ex.DiscordCode}{Environment.NewLine}Here's exception message: {ex.Message}");
                Task.Delay(10000).Wait();
                Environment.Exit(0);
            }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());     // Load commands and modules into the command service
            await new ExtensionLoadingService(_discord, _commands, _config, _provider).Load().ConfigureAwait(false);
            SqLite.Start();
        }
    }
}
