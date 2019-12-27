using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.ServerBackup.Models
{
    class BackupEmbed
    {
        public class BackupEmbedField
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Inline { get; set; }
            public BackupEmbedField(SocketGuild g, EmbedField f)
            {
                this.Name = f.Name;
                this.Value = BackupGuild.FixId(g, f.Value).GetAwaiter().GetResult();
                this.Inline = f.Inline;
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
            this.Title = e.Title;
            this.Description = BackupGuild.FixId(g, e.Description).GetAwaiter().GetResult();
            this.Url = e.Url;
            this.Color = e.Color;
            this.Timestamp = e.Timestamp;
            this.FooterIconUrl = e.Footer.HasValue ? e.Footer.Value.IconUrl : null;
            this.FooterText = e.Footer.HasValue ? e.Footer.Value.Text : null;
            this.Thumbnail = e.Thumbnail.HasValue ? e.Thumbnail.Value.Url : null;
            this.Image = e.Image.HasValue ? e.Image.Value.Url : null;
            this.Author = e.Author.HasValue ? e.Author.Value.Name : null;
            this.AuthorUrl = e.Author.HasValue ? e.Author.Value.Url : null;
            this.AuthorIcon = e.Author.HasValue ? e.Author.Value.IconUrl : null;
            this.Fields = e.Fields.Select(f => new BackupEmbedField(g, f)).ToArray();
        }
    }
}

