using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Module.Default
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "Default";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => null;

        public void PreLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {}

        public void PostLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {}
    }
}
