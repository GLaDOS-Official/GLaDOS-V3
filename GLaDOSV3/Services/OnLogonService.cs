using Discord;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    public class OnLogonService
    {
        private readonly BotSettingsHelper<string> botSettingsHelper;

        // DiscordShardedClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public OnLogonService(
            DiscordShardedClient discord,
            BotSettingsHelper<string> botSettingsHelper)
        {
            if (discord == null) return;
            discord.ShardConnected += this.ShardConnected;
            this.botSettingsHelper      =  botSettingsHelper;
        }


        private async Task ShardConnected(DiscordSocketClient client)
        {
            await IsMfaEnabled(client).ConfigureAwait(false);
            await this.GetUserFromConfigAsync(client).ConfigureAwait(false);

            if (this.botSettingsHelper["discord_status"] != "online")
            {
                if (Enum.TryParse(typeof(UserStatus), this.botSettingsHelper["discord_status"], true, out var status))
                    await client.SetStatusAsync((UserStatus)status).ConfigureAwait(false);
                else
                    await LoggingService.Log(LogSeverity.Warning, "Client status",
                        "Could not parse status string from database!").ConfigureAwait(false);
            }
            //TODO: fix
            //client.SetActivityAsync(IActivity)
            //if (client.CurrentUser.Activity?.Name != this.botSettingsHelper["discord_game"])
            //    await client.SetGameAsync(this.botSettingsHelper["discord_game"]).ConfigureAwait(false);
        }
        private static Task<bool> IsMfaEnabled(DiscordSocketClient client)
        {
            if (client.CurrentUser == null) return Task.FromResult(false);
            if (client.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            LoggingService.Log(LogSeverity.Warning, "Bot",
                "MFA is disabled! Mod usage on MFA enabled server won't work!").GetAwaiter();
            return Task.FromResult(false);
        }
        private async Task GetUserFromConfigAsync(DiscordSocketClient client)
        {
            if (client.CurrentUser == null) return;
            if (client.CurrentUser.Username != this.botSettingsHelper["name"])
            {
                await client.CurrentUser.ModifyAsync(u => u.Username = this.botSettingsHelper["name"]).ConfigureAwait(false);
                foreach (SocketGuild guild in client.Guilds)
                {
                    var me = guild.GetUser(client.CurrentUser.Id);
                    if (me.Nickname == this.botSettingsHelper["name"]) continue;
                    await me.ModifyAsync(x => x.Nickname = this.botSettingsHelper["name"]).ConfigureAwait(false);
                }
            }
        }
    }
}
