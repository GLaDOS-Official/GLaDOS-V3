using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using GladosV3.Helpers;
using Victoria;

namespace GladosV3.Module.Music
{
    public class ModuleInfo : GladosV3.IGladosModule
    {
        public string Name() => "Music";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => new Type[] { typeof(LavaRestClient), typeof(LavaSocketClient), typeof(AudioService)};
        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
        }
        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
        }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }
    }
}
