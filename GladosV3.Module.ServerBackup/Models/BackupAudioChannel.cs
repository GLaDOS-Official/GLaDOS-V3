using Discord.WebSocket;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupAudioChannel : BackupChannel
    {
        public int? UserLimit { get; set; }
        public string Category { get; set; }
        public int Bitrate { get; set; }

        public BackupAudioChannel(SocketVoiceChannel c, ref int channelId) : base(c, ref channelId)
        {
            if (c == null) return;
            UserLimit = c.UserLimit ?? 0;
            Category = c.Category?.Name;
            Bitrate = c.Bitrate;
        }
    }
}
