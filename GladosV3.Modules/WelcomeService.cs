using Discord;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace GladosV3.Module.Default
{
    internal class WelcomeService
    {

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public WelcomeService(DiscordSocketClient discord)
        {
            discord.UserJoined += this.DiscordOnUserJoined;
            discord.UserLeft += this.DiscordOnUserLeft;
        }

        private async Task DiscordOnUserJoined(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            using var db = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString(CultureInfo.InvariantCulture)}'").ConfigureAwait(true);
            if (Convert.ToInt32(db.Rows[0]["join_toggle"], CultureInfo.InvariantCulture) == 1)
            {
                var text = await this.FormatText(socketGuildUser, db.Rows[0]["join_msg"].ToString()).ConfigureAwait(true);
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture))).SendMessageAsync(text).ConfigureAwait(false);
                else
                {
                    await guild.Owner.SendMessageAsync($"I tried to send a welcome message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.").ConfigureAwait(false); await this.Disable(guild).ConfigureAwait(false);
                }
            }
        }

        private async Task DiscordOnUserLeft(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            using var db = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString(CultureInfo.InvariantCulture)}'").ConfigureAwait(true);
            if (Convert.ToInt32(db.Rows[0]["leave_toggle"], CultureInfo.InvariantCulture) == 1)
            {
                var text = await this.FormatText(socketGuildUser, db.Rows[0]["leave_msg"].ToString()).ConfigureAwait(true);
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)))
                        .SendMessageAsync(text).ConfigureAwait(false);
                else
                {
                    await guild.Owner
                        .SendMessageAsync($"I tried to send a farewell message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.").ConfigureAwait(false);
                    await this.Disable(guild).ConfigureAwait(false);
                }
            }
        }

        private Task Disable(SocketGuild guild)
        {
            SqLite.Connection.SetValueAsync("servers", "join_toggle", 0, $"WHERE guildid={guild.Id.ToString(CultureInfo.InvariantCulture)}");
            SqLite.Connection.SetValueAsync("servers", "leave_toggle", 0, $"WHERE guildid={guild.Id.ToString(CultureInfo.InvariantCulture)}");
            return Task.CompletedTask;
        }

        private Task<string> FormatText(SocketGuildUser user, string text) =>
            Task.FromResult(text.Replace("{mention}", $"<@{user.Id}>", StringComparison.Ordinal)
                                .Replace("{uname}", user.Username, StringComparison.Ordinal)
                                .Replace("{sname}", user.Guild.Name, StringComparison.Ordinal)
                                .Replace("{count}", user.Guild.MemberCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }
}
