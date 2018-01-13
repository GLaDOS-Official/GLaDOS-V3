using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Services
{
    class WelcomeService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public WelcomeService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            discord.UserJoined += DiscordOnUserJoined;
            discord.UserLeft += DiscordOnUserLeft;
        }

        private async Task DiscordOnUserJoined(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            var db = SqLite.Connection.GetValues("servers", guild.Id.ToString());
            if (Convert.ToInt32(db.Rows[0]["join_toggle"]) == 1)
            {
                var text = await FormatText(socketGuildUser, db.Rows[0]["join_msg"].ToString());
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])))
                        .SendMessageAsync(text);
                else
                {
                    await ((IUser)guild.Owner)
                        .SendMessageAsync("I tried to send a welcome message to a channel, but it now longer exists. Please set this up again.");
                    Disable(guild);
                }
            }
        }

        private async Task DiscordOnUserLeft(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            var db = SqLite.Connection.GetValues("servers", guild.Id.ToString());
            if (Convert.ToInt32(db.Rows[0]["leave_toggle"]) == 1)
            {
                var text = await FormatText(socketGuildUser, db.Rows[0]["leave_msg"].ToString());
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"])))
                        .SendMessageAsync(text);
                else
                {
                    await ((IUser)guild.Owner)
                        .SendMessageAsync("I tried to send a welcome message to a channel, but it now longer exists. Please set this up again.");
                    await Disable(guild);
                }
            }
        }

        private Task Disable(SocketGuild guild)
        {
            SqLite.Connection.SetValue("servers", "join_toggle", 0, guild.Id.ToString());
            SqLite.Connection.SetValue("servers", "leave_toggle", 0, guild.Id.ToString());
            return Task.CompletedTask;
        }

        private Task<string> FormatText(SocketGuildUser user, string text)
        {
            return Task.FromResult(text.Replace("{mention}", $"<@{user.Id}>")
            .Replace("{uname}", user.Username)
            .Replace("{sname}", user.Guild.Name)
            .Replace("{count}", user.Guild.MemberCount.ToString()));
        }
    }
}
