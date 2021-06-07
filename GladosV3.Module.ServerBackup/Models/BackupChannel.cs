using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace GLaDOSV3.Module.ServerBackup.Models
{

    public class BackupChannel
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public bool IsHidden { get; set; }
        public int LocalChannelId { get; set; }
        public List<BackupChannelPerms> Permissions { get; set; }
        public BackupChannel(SocketGuildChannel c, ref int channelId)
        {
            if (c == null) return;
            LocalChannelId = ++channelId;
            Name = c.Name;
            Position = c.Position;
            IsHidden = c.GetUser(c.Guild.CurrentUser.Id) == null ? true : !c.GetUser(c.Guild.CurrentUser.Id).GetPermissions(c).ViewChannel;
            Permissions = c.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(z => new BackupChannelPerms(z, c.Guild)).ToList();
        }
    }
}
