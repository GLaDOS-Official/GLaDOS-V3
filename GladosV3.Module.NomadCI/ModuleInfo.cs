using System;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Module.NomadCI
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "NomadCI";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => null;

        public void PreLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {
            BuilderService.config = Tools.GetConfigAsync(1).GetAwaiter().GetResult();
            BuilderService.Service = new BuilderService();
        }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {
            BuilderService.client = discord;
            BuilderService.client.Ready += BuilderService.LoadCIChannel;
        }
    }
}
