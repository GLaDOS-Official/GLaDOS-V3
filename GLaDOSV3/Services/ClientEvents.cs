using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

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
        private async Task JoinedGuild(SocketGuild arg)
        {
            SqLite.Connection.AddRecord("servers","guildid",new []{ arg.Id.ToString()});
        }
        private async Task LeftGuild(SocketGuild arg)
        {
            SqLite.Connection.RemoveRecord("servers",$"guildid={arg.Id.ToString()}");
        }
    }
}
