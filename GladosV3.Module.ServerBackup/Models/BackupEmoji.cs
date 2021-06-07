using Discord;
using System.Net;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupEmoji
    {
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public BackupEmoji(GuildEmote e)
        {
            if (e == null) return;
            Name = e.Name;
            using var wc = new WebClient();
            Image = wc.DownloadData(e.Url);
        }
    }
}
