using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3
{
    public interface IGladosModule
    {
        string Name();
        string Version();
        string UpdateUrl();
        string Author();
        void OnLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider);
    }
}