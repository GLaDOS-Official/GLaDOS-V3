using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladosV3.Module.ServerBackup.Models
{
    
    class BackupAudioChannel : BackupChannel
    {
        public int? UserLimit { get; set; }
        public string Category { get; set; }
        public int Bitrate { get; set; }
        
        public BackupAudioChannel(SocketVoiceChannel c, ref int channelId) : base(c, ref channelId)
        {
            if (c == null) return;
            this.UserLimit = c.UserLimit ?? 0;
            this.Category = c.Category?.Name;
            this.Bitrate = c.Bitrate;
        }
    }
}
