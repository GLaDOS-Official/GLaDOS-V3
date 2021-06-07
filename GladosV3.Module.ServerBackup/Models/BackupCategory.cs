using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupCategory : BackupChannel
    {
        public List<BackupTextChannel> TextChannels { get; set; }
        public List<BackupAudioChannel> VoiceChannels { get; set; }
        public BackupCategory(SocketCategoryChannel c, ref int channelId) : base(c, ref channelId)
        {
            if (c == null) return;
            var fuck = channelId;
            TextChannels = c.Channels.Where((f, c) => f is SocketTextChannel).OrderBy(c => c.Id).Select(c => new BackupTextChannel((SocketTextChannel)c, ref fuck)).ToList();
            VoiceChannels = c.Channels.Where((f, c) => f is SocketVoiceChannel).OrderBy(c => c.Id).Select(c => new BackupAudioChannel((SocketVoiceChannel)c, ref fuck)).ToList();
            channelId = fuck;
        }
    }
}
