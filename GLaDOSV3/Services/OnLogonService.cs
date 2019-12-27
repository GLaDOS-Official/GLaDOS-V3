using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Threading.Tasks;
using System.IO;

namespace GladosV3.Services
{
    public class OnLogonService
    {
        private readonly DiscordSocketClient _discord;
        private readonly BotSettingsHelper<string> _botSettingsHelper;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public OnLogonService(
            DiscordSocketClient discord,
            BotSettingsHelper<string> botSettingsHelper)
        {
            _discord = discord;
            _discord.Connected += Connected;
            _botSettingsHelper = botSettingsHelper;
        }

        private async Task Connected()
        {
            await IsMfaEnabled();
            await GetUserFromConfigAsync();

            if (_botSettingsHelper["discord_status"] != "online")
            {
                if (Enum.TryParse(typeof(UserStatus), _botSettingsHelper["discord_status"], true, out var status))
                    await _discord.SetStatusAsync((UserStatus)status);
                else
                    await LoggingService.Log(LogSeverity.Warning, "Client status",
                        "Could not parse status string from config.json!");
            }

            if (_discord.CurrentUser.Activity?.Name != _botSettingsHelper["discord_game"])
                await _discord.SetGameAsync(_botSettingsHelper["discord_game"]);
            var pfpPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "avatar.img");
            if (File.Exists(pfpPath))
                await _discord.CurrentUser.ModifyAsync((f) => f.Avatar = new Image(pfpPath));
        }
        private Task<bool> IsMfaEnabled()
        {
            if (_discord.CurrentUser == null) return Task.FromResult(false);
            if (_discord.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            LoggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! Mod usage on MFA enabled server won't work!",
                null).GetAwaiter();
            return Task.FromResult(false);
        }
        private async Task GetUserFromConfigAsync()
        {
            if (_discord.CurrentUser == null) return;
            if (_discord.CurrentUser.Username != _botSettingsHelper["name"])
            {
                await _discord.CurrentUser.ModifyAsync(u => u.Username = _botSettingsHelper["name"]);
                foreach (SocketGuild guild in _discord.Guilds)
                {
                    var me = guild.GetUser(_discord.CurrentUser.Id);
                    if (me.Nickname == _botSettingsHelper["name"]) continue;
                    await me.ModifyAsync(x =>
                    {
                        x.Nickname = _botSettingsHelper["name"];
                    });
                }
            }
        }
    }
}
