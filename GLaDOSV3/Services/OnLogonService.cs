using System;
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
            await GetUserFromConfigAsync();
            if (_config["discord:status"] != "online")  {
                if (Enum.TryParse(typeof(UserStatus), _config["discord:status"], true, out var status))
                    await _discord.SetStatusAsync((UserStatus)status);
                else
                    await LoggingService.Log(LogSeverity.Warning, "Client status",
                        "Could not parse status string from config.json!");
            }
            if (_discord.CurrentUser.Game?.Name != _config["discord:game"])
                await _discord.SetGameAsync(_config["discord:game"]);
        }
        private Task<bool> IsMfaEnabled()
        {
            if (_discord.CurrentUser == null) return Task.FromResult(false);
            if (_discord.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            LoggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! Mod usage might not work!",
                null).GetAwaiter();
            return Task.FromResult(false);
        }
        private async Task GetUserFromConfigAsync()
        {
            if (_discord.CurrentUser == null) return;
            if (_discord.CurrentUser.Username != _config["name"])   
            {
                await _discord.CurrentUser.ModifyAsync(u => u.Username = _config["name"]);
                foreach (SocketGuild guild in _discord.Guilds)
                {
                    var me = guild.GetUser(_discord.CurrentUser.Id);
                    if (me.Nickname == _config["name"]) continue;
                    await me.ModifyAsync(x =>
                    {
                        x.Nickname = _config["name"];
                    });
                }
            }
        }
    }
}
