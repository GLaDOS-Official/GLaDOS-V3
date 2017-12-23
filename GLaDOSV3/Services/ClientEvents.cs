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
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public ClientEvents(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            _discord.JoinedGuild += JoinedGuild;
            _discord.LeftGuild += LeftGuild;
        }

        private async Task JoinedGuild(SocketGuild arg)
        {
            string sql = $"INSERT INTO servers (guildid,nsfw) VALUES (@val1, @va2);";

            SQLiteCommand command = new SQLiteCommand(sql, SqLite.Connection);
            command.Parameters.AddWithValue("@val1", arg.Id);
            command.Parameters.AddWithValue("@val2", false);
            command.ExecuteNonQuery();
        }
        private async Task LeftGuild(SocketGuild arg)
        {
            string sql = $"DELETE FROM servers WHERE guildid=@val1;";

            SQLiteCommand command = new SQLiteCommand(sql, SqLite.Connection);
            command.Parameters.AddWithValue("@val1", arg.Id);
            command.ExecuteNonQuery();
        }
    }
}
