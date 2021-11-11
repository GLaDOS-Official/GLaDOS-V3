using Discord;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    public class OnLogonService
    {
        private readonly ILogger<OnLogonService> _logger;
        private readonly BotSettingsHelper<string> botSettingsHelper;
        // DiscordShardedClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public OnLogonService(
            DiscordShardedClient discord,
            BotSettingsHelper<string> botSettingsHelper,
            ILogger<OnLogonService> logger)
        {
            if (discord == null) return;
            discord.ShardConnected += this.ShardConnected;
            this.botSettingsHelper = botSettingsHelper;
            this._logger = logger;
        }


        private async Task ShardConnected(DiscordSocketClient client)
        {
            await this.IsMfaEnabled(client).ConfigureAwait(false);
            await this.GetUserFromConfigAsync(client).ConfigureAwait(false);

            if (this.botSettingsHelper["discord_status"] != "online")
            {
                if (Enum.TryParse(typeof(UserStatus), this.botSettingsHelper["discord_status"], true, out var status))
                    await client.SetStatusAsync((UserStatus)status).ConfigureAwait(false);
                else
                    this._logger.LogWarning("[Client status] Could not parse status string from database!");
            }
            if (client.CurrentUser.Activities.Count == 0 || client.CurrentUser.Activities.First()?.Name != this.botSettingsHelper["discord_game"])
                await client.SetActivityAsync(new Game(this.botSettingsHelper["discord_game"], ActivityType.Playing));
        }
        private Task<bool> IsMfaEnabled(DiscordSocketClient client)
        {
            if (client.CurrentUser == null) return Task.FromResult(false);
            if (client.CurrentUser.IsMfaEnabled) return Task.FromResult(true);
            this._logger.LogWarning("MFA is disabled! Mod usage on MFA enabled server won't work!");
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
                    SocketGuildUser me = guild.GetUser(client.CurrentUser.Id);
                    if (me.Nickname == this.botSettingsHelper["name"]) continue;
                    await me.ModifyAsync(x => x.Nickname = this.botSettingsHelper["name"]).ConfigureAwait(false);
                }
            }
        }
    }
}
