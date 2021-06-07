using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupEmbed
    {
        public class BackupEmbedField
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Inline { get; set; }
            public BackupEmbedField(SocketGuild g, EmbedField f)
            {
                Name = f.Name;
                Value = BackupGuild.FixId(g, f.Value).GetAwaiter().GetResult();
                Inline = f.Inline;
            }
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public Color? Color { get; set; }
        public DateTimeOffset? Timestamp;
        public string FooterIconUrl { get; set; }
        public string FooterText { get; set; }
        public string Thumbnail { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string AuthorIcon { get; set; }
        public BackupEmbedField[] Fields { get; set; }
        public BackupEmbed(SocketGuild g, IEmbed e)
        {
            if (e == null) return;
            Title = e.Title;
            Description = BackupGuild.FixId(g, e.Description).GetAwaiter().GetResult();
            Url = e.Url;
            Color = e.Color;
            this.Timestamp = e.Timestamp;
            FooterIconUrl = e.Footer.HasValue ? e.Footer.Value.IconUrl : null;
            FooterText = e.Footer.HasValue ? e.Footer.Value.Text : null;
            Thumbnail = e.Thumbnail.HasValue ? e.Thumbnail.Value.Url : null;
            Image = e.Image.HasValue ? e.Image.Value.Url : null;
            Author = e.Author.HasValue ? e.Author.Value.Name : null;
            AuthorUrl = e.Author.HasValue ? e.Author.Value.Url : null;
            AuthorIcon = e.Author.HasValue ? e.Author.Value.IconUrl : null;
            Fields = e.Fields.Select(f => new BackupEmbedField(g, f)).ToArray();
        }
    }
}

