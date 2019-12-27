using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.ServerBackup.Models
{
    
    class BackupCategory : BackupChannel
    {
        public List<BackupTextChannel> TextChannels { get; set; }
        public List<BackupAudioChannel> VoiceChannels { get; set; }
        public BackupCategory(SocketCategoryChannel c, ref int channelId) : base(c, ref channelId)
        {
            if (c == null) return;
            int fuck = channelId;
            this.TextChannels =  c.Channels.Where((f, c) => f is SocketTextChannel).OrderBy(c => c.Id).Select(c => new BackupTextChannel((SocketTextChannel)c, ref fuck)).ToList();
            this.VoiceChannels = c.Channels.Where((f, c) => f is SocketVoiceChannel).OrderBy(c => c.Id).Select(c => new BackupAudioChannel((SocketVoiceChannel)c, ref fuck)).ToList();
            channelId = fuck;
        }
    }
}
