using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Services
{
    public class OnLogonService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public OnLogonService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            _discord.Connected += Connected;
        }

        private async Task Connected()
        {
            await IsMfaEnabled();
        }
        private Task<bool> IsMfaEnabled()
        {
            if (_discord.CurrentUser == null) return Task.FromResult(false);
            else if (_discord.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            var loggingService = new LoggingService(_discord, this._commands,false);
            loggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! This means that your bot will be unable to gather Administator, Manage server & roles & channels & messages & webhooks, kick and ban members!",
                null).GetAwaiter();
            return Task.FromResult(false);
        }
    }
}
