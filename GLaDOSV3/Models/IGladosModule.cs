using System;
using System.Reflection;
using System.Runtime.Loader;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace GLaDOSV3.Models.Interfaces
{
    public interface IGladosModule
    {
        string Name();
        string Version();
        Uri UpdateUrl();
        string Author();
        DateTime GetCompileTime() => Builtin.CompileTime;
        Type[] Services(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceCollection provider);
        void PreLoad(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void PostLoad(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Reload(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Unload(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
    }
}