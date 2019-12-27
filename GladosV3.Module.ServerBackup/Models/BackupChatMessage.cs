using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.ServerBackup.Models
{

    class BackupChatMessage
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public ulong AuthorId { get; set; }
        public string AuthorPic { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public BackupEmbed[] Embeds { get; set; }
        public bool HasAttachments { get; set; }
        public BackupChatMessage(IMessage msg)
        {
            if (msg == null) return;
            this.Text = BackupGuild.FixId(((SocketTextChannel)msg.Channel).Guild, msg.Content).GetAwaiter().GetResult();
            this.Author = msg.Author.Username;
            this.AuthorId = msg.Author.Id;
            this.AuthorPic = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl();
            this.Timestamp = msg.Timestamp;
            this.HasAttachments = msg.Attachments.Count > 0;
            this.Embeds = msg.Embeds.Where(e => e.Type == EmbedType.Rich).Select(e => new BackupEmbed(((SocketTextChannel)msg.Channel).Guild, e)).ToArray();
        }
    }
}
