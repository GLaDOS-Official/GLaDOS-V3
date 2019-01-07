using System.Threading.Tasks;
using Discord.WebSocket;
using GladosV3.Helpers;

namespace GladosV3.Services
{
    public class ClientEvents
    {

        // DiscordSocketClient is injected automatically from the IServiceProvider
        public ClientEvents(
            DiscordSocketClient discord)
        {
           discord.JoinedGuild += JoinedGuild;
            discord.LeftGuild += LeftGuild;
        }
        private Task JoinedGuild(SocketGuild arg)
        {
            return SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new []{ arg.Id.ToString(),"0","0","0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}"});
        }
        private Task LeftGuild(SocketGuild arg)
        {
            return SqLite.Connection.RemoveRecordAsync("servers",$"guildid={arg.Id.ToString()}");
        }
    }
}