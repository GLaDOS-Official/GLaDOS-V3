namespace GladosV3.Module.FTPServer
{
    public class ModuleInfo : IGladosModule
    {
        public string Name()
        {
            return "FTPServer";
        }

        public string Version()
        {
            return "0.0.0.1";
        }

        public string UpdateUrl()
        {
            return null;
        }

        public string Author()
        {
            return "BlackOfWorld#8125";
        }

        public Type[] Services => null;

        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
            Main.StartFTP();
        }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }
    }
}
