using Discord;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

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
            this._discord = discord;
            this._discord.Connected += this.Connected;
            this._botSettingsHelper = botSettingsHelper;
        }

        private async Task Connected()
        {
            await this.IsMfaEnabled();
            await this.GetUserFromConfigAsync();

            if (this._botSettingsHelper["discord_status"] != "online")
            {
                if (Enum.TryParse(typeof(UserStatus), this._botSettingsHelper["discord_status"], true, out var status))
                    await this._discord.SetStatusAsync((UserStatus)status);
                else
                    await LoggingService.Log(LogSeverity.Warning, "Client status",
                        "Could not parse status string from config.json!");
            }

            if (this._discord.CurrentUser.Activity?.Name != this._botSettingsHelper["discord_game"])
                await this._discord.SetGameAsync(this._botSettingsHelper["discord_game"]);
            var pfpPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "avatar.img");
            if (File.Exists(pfpPath))
                await this._discord.CurrentUser.ModifyAsync((f) => f.Avatar = new Image(pfpPath));
        }
        private Task<bool> IsMfaEnabled()
        {
            if (this._discord.CurrentUser == null) return Task.FromResult(false);
            if (this._discord.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            LoggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! Mod usage on MFA enabled server won't work!",
                null).GetAwaiter();
            return Task.FromResult(false);
        }
        private async Task GetUserFromConfigAsync()
        {
            if (this._discord.CurrentUser == null) return;
            if (this._discord.CurrentUser.Username != this._botSettingsHelper["name"])
            {
                await this._discord.CurrentUser.ModifyAsync(u => u.Username = this._botSettingsHelper["name"]);
                foreach (SocketGuild guild in this._discord.Guilds)
                {
                    var me = guild.GetUser(this._discord.CurrentUser.Id);
                    if (me.Nickname == this._botSettingsHelper["name"]) continue;
                    await me.ModifyAsync(x =>
                    {
                        x.Nickname = this._botSettingsHelper["name"];
                    });
                }
            }
        }
    }
}
