using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System.Globalization;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    public class ClientEvents
    {
        public ClientEvents(
            DiscordSocketClient discord)
        {
            if (discord == null) return;
            discord.JoinedGuild += this.JoinedGuild;
            discord.LeftGuild += this.LeftGuild;
        }
        private Task JoinedGuild(SocketGuild arg)
        {
            if (CommandHandler.BlacklistedServers.Contains(arg.Id))
            {
                arg.DefaultWritableChannel().SendMessageAsync($"Hello! This server has been blacklisted from using {arg.CurrentUser.Mention}! I will now leave. Have fun without me!");
                arg.LeaveAsync().GetAwaiter();
                return Task.CompletedTask;
            }
            Task status = SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { arg.Id.ToString(CultureInfo.InvariantCulture), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" });
            return status;
        }
        private Task LeftGuild(SocketGuild arg) => CommandHandler.BlacklistedServers.Contains(arg.Id) ? Task.CompletedTask : SqLite.Connection.RemoveRecordAsync("servers", $"guildid={arg.Id.ToString(CultureInfo.InvariantCulture)}");
    }
}