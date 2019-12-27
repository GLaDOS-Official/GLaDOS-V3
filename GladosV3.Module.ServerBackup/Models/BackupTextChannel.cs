using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GladosV3.Module.ServerBackup.Models
{

    class BackupTextChannel : BackupChannel
    {
        public bool IsNSFW { get; set; }
        public string Category { get; set; }
        public string Topic { get; set; }
        public List<BackupChatMessage> LastMessages { get; set; }
        public int Slowmode { get; set; }
        public BackupTextChannel(SocketTextChannel c, ref int channelId) : base(c, ref channelId)
        {
            if (c == null) return;
            this.IsNSFW = c.IsNsfw;
            this.Category = c.Category?.Name;
            this.Topic = BackupGuild.FixId(c.Guild, c.Topic).GetAwaiter().GetResult();
            this.LastMessages = GetMessages(c, 250).GetAwaiter().GetResult();
            this.Slowmode = c.SlowModeInterval;
        }
        private async Task<List<BackupChatMessage>> GetMessages(ITextChannel channel, int msgCount)
        {
            var list = new List<BackupChatMessage>(msgCount);
            if (this.IsHidden) return list;
            list.AddRange((await channel.GetMessagesAsync(msgCount).FlattenAsync()).Where(msg => msg.Source != MessageSource.System).Select(item => new BackupChatMessage(item)));
            list.Reverse();
            return list;
        }
    }
}
