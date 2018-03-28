using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Module.Music
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "Music";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => null;

        public void PreLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config,
            IServiceProvider provider)
        {
            AudioService.service = new AudioService(config);
        }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config,
            IServiceProvider provider)
        {
            discord.UserVoiceStateUpdated += (user, old, _new) =>
            {
                if (old.VoiceChannel == null)
                    return Task.CompletedTask;
                if (!AudioService.ConnectedChannels.TryGetValue(old.VoiceChannel.Guild.Id, out MusicClass mclass))
                    return Task.CompletedTask;
                if (old.VoiceChannel.Id == mclass.VCID && old.VoiceChannel.Users.Count <= 1)
                    AudioService.service.LeaveAudioAsync(old.VoiceChannel.Guild).GetAwaiter();
                return Task.CompletedTask;
            };
        }
    }
}
