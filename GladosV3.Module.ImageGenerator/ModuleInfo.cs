using Discord.Commands;
using Discord.WebSocket;
using System;
using GladosV3.Helpers;

namespace GladosV3.Module.ImageGeneration
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "ImageGenerator";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => new[] { typeof(GeneratorService) };

        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider provider)
        { }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }
    }
}
