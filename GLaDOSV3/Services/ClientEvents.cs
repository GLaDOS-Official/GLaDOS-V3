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
            return SqLite.Connection.AddRecordAsync("servers","guildid",new []{ arg.Id.ToString()});
        }
        private Task LeftGuild(SocketGuild arg)
        {
            return SqLite.Connection.RemoveRecordAsync("servers",$"guildid={arg.Id.ToString()}");
        }
    }
}