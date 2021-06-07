using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupChatMessage
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public ulong AuthorId { get; set; }
        public string AuthorPic { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public BackupEmbed[] Embeds { get; set; }
        public bool HasAttachments { get; set; }
        public bool IsPinned { get; set; }
        public BackupChatMessage(IMessage msg)
        {
            if (msg == null) return;
            Text = BackupGuild.FixId(((SocketTextChannel)msg.Channel).Guild, msg.Content).GetAwaiter().GetResult();
            Author = msg.Author.Username;
            AuthorId = msg.Author.Id;
            AuthorPic = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl();
            Timestamp = msg.Timestamp;
            HasAttachments = msg.Attachments.Count > 0;
            Embeds = msg.Embeds.Where(e => e.Type == EmbedType.Rich).Select(e => new BackupEmbed(((SocketTextChannel)msg.Channel).Guild, e)).ToArray();
            IsPinned = msg.IsPinned;
        }
    }
}
