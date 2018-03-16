using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Discord.Net;
using GladosV3.Helpers;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            DateCompiled.WriteToConsole();
            string discordToken = _config["tokens:discord"];     // Get the discord token from the config file
            string gameTitle = _config["discord:game"]; // Get bot's game status
            if (string.IsNullOrWhiteSpace(discordToken) || string.IsNullOrEmpty(discordToken))
                throw new Exception("Please enter your bot's token into the `_configuration.json` file found in the applications root directory.");
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
                    Helpers.Tools.WriteColorLine(ConsoleColor.Red, "Wrong or invalid token.");
                else if (ex.DiscordCode == 502 || ex.HttpCode == HttpStatusCode.BadGateway)
                    Helpers.Tools.WriteColorLine(ConsoleColor.Yellow, "Gateway unavailable.");
                else if (ex.DiscordCode == 400 || ex.HttpCode == HttpStatusCode.BadRequest)
                    Helpers.Tools.WriteColorLine(ConsoleColor.Red, "Bad request. Please wait for an update.");
                Helpers.Tools.WriteColorLine(ConsoleColor.Red,
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
