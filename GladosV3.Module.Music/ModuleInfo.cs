using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using GladosV3.Helpers;

namespace GladosV3.Module.Music
{
    public class ModuleInfo : GladosV3.IGladosModule
    {
        public string Name() => "Music";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => null;
        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
            AudioService.service = new AudioService(config);
        }
        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
            discord.UserVoiceStateUpdated += (user, old, _new) =>
            {
                if (old.VoiceChannel == null)
                    return Task.CompletedTask;
                if (!AudioService.ConnectedChannels.TryGetValue(old.VoiceChannel.Guild.Id, out MusicClass mclass))
                    return Task.CompletedTask;
                if (old.VoiceChannel.Id == mclass.VoiceChannelID && old.VoiceChannel.Users.Count <= 1)
                    AudioService.service.LeaveAudioAsync(old.VoiceChannel.Guild).GetAwaiter();
                return Task.CompletedTask;
            };
        }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }
    }
}
