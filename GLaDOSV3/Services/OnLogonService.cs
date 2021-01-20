using Discord;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    public class OnLogonService
    {
        private readonly DiscordSocketClient discord;
        private readonly BotSettingsHelper<string> botSettingsHelper;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public OnLogonService(
            DiscordSocketClient discord,
            BotSettingsHelper<string> botSettingsHelper)
        {
            if (discord == null) return;
            this.discord = discord;
            this.discord.Connected += this.Connected;
            this.botSettingsHelper = botSettingsHelper;
        }

        private async Task Connected()
        {
            await this.IsMfaEnabled().ConfigureAwait(false);
            await this.GetUserFromConfigAsync().ConfigureAwait(false);

            if (this.botSettingsHelper["discord_status"] != "online")
            {
                if (Enum.TryParse(typeof(UserStatus), this.botSettingsHelper["discord_status"], true, out var status))
                    await this.discord.SetStatusAsync((UserStatus)status).ConfigureAwait(false);
                else
                    await LoggingService.Log(LogSeverity.Warning, "Client status",
                        "Could not parse status string from database!").ConfigureAwait(false);
            }

            if (this.discord.CurrentUser.Activity?.Name != this.botSettingsHelper["discord_game"])
                await this.discord.SetGameAsync(this.botSettingsHelper["discord_game"]).ConfigureAwait(false);
        }
        private Task<bool> IsMfaEnabled()
        {
            if (this.discord.CurrentUser == null) return Task.FromResult(false);
            if (this.discord.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            LoggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! Mod usage on MFA enabled server won't work!",
                null).GetAwaiter();
            return Task.FromResult(false);
        }
        private async Task GetUserFromConfigAsync()
        {
            if (this.discord.CurrentUser == null) return;
            if (this.discord.CurrentUser.Username != this.botSettingsHelper["name"])
            {
                await this.discord.CurrentUser.ModifyAsync(u => u.Username = this.botSettingsHelper["name"]).ConfigureAwait(false);
                foreach (SocketGuild guild in this.discord.Guilds)
                {
                    var me = guild.GetUser(this.discord.CurrentUser.Id);
                    if (me.Nickname == this.botSettingsHelper["name"]) continue;
                    await me.ModifyAsync(x => x.Nickname = this.botSettingsHelper["name"]).ConfigureAwait(false);
                }
            }
        }
    }
}
