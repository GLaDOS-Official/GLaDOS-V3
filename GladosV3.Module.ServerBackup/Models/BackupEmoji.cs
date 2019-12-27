using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GladosV3.Module.ServerBackup.Models
{
    
    class BackupEmoji
    {
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public BackupEmoji(GuildEmote e)
        {
            if (e == null) return;
            this.Name = e.Name;
            using var wc = new WebClient();
            this.Image = wc.DownloadData(e.Url);
        }
    }
}
