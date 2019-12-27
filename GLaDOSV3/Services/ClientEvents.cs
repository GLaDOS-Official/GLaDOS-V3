using Discord.WebSocket;
using GladosV3.Helpers;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    public class ClientEvents
    {
        private readonly DiscordSocketClient _discord;
        // DiscordSocketClient is injected automatically from the IServiceProvider
        public ClientEvents(
            DiscordSocketClient discord)
        {
            _discord = discord;
            discord.JoinedGuild += JoinedGuild;
            discord.LeftGuild += LeftGuild;
        }
        private Task JoinedGuild(SocketGuild arg)
        {
            if (CommandHandler.BlacklistedServers.Contains(arg.Id))
            {
                arg.DefaultChannel.SendMessageAsync(
                    $"Hello! This server has been blacklisted from using {_discord.CurrentUser.Mention}! I will no leave. Have fun without me!");
                arg.LeaveAsync().GetAwaiter();
                return Task.CompletedTask;
            }
            Task status = SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { arg.Id.ToString(), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" });
            return status;
        }
        private Task LeftGuild(SocketGuild arg)
        {
            return CommandHandler.BlacklistedServers.Contains(arg.Id) ? Task.CompletedTask : SqLite.Connection.RemoveRecordAsync("servers", $"guildid={arg.Id.ToString()}");
        }
    }
}