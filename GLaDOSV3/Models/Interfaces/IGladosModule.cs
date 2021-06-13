using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace GLaDOSV3
{
    public interface IGladosModule
    {
        string Name();
        string Version();
        Uri UpdateUrl();
        string Author();
        Type[] Services(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceCollection provider);
        void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
        void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider);
    }
}