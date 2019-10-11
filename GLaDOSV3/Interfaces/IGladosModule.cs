using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Reflection;
using System.Runtime.Loader;

namespace GladosV3
{
    public interface IGladosModule
    {
        string Name();
        string Version();
        string UpdateUrl();
        string Author();
        Type[] Services { get; }
        void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
    }
}