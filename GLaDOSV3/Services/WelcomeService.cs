using Discord;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Threading.Tasks;

namespace GladosV3.Services
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
            var db = SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString()}'").GetAwaiter().GetResult();
            if (Convert.ToInt32(db.Rows[0]["join_toggle"]) == 1)
            {
                var text = await this.FormatText(socketGuildUser, db.Rows[0]["join_msg"].ToString());
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])))
                        .SendMessageAsync(text);
                else
                {
                    await guild.Owner
                        .SendMessageAsync($"I tried to send a welcome message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.");
                    await this.Disable(guild);
                }
            }
        }

        private async Task DiscordOnUserLeft(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            var db = SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString()}'").GetAwaiter().GetResult();
            if (Convert.ToInt32(db.Rows[0]["leave_toggle"]) == 1)
            {
                var text = await this.FormatText(socketGuildUser, db.Rows[0]["leave_msg"].ToString());
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])))
                        .SendMessageAsync(text);
                else
                {
                    await guild.Owner
                        .SendMessageAsync($"I tried to send a farewell message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.");
                    await this.Disable(guild);
                }
            }
        }

        private Task Disable(SocketGuild guild)
        {
            SqLite.Connection.SetValueAsync("servers", "join_toggle", 0, $"WHERE guildid={guild.Id.ToString()}");
            SqLite.Connection.SetValueAsync("servers", "leave_toggle", 0, $"WHERE guildid={guild.Id.ToString()}");
            return Task.CompletedTask;
        }

        private Task<string> FormatText(SocketGuildUser user, string text) =>
            Task.FromResult(text.Replace("{mention}", $"<@{user.Id}>")
                                .Replace("{uname}", user.Username)
                                .Replace("{sname}", user.Guild.Name)
                                .Replace("{count}", user.Guild.MemberCount.ToString()));
    }
}
