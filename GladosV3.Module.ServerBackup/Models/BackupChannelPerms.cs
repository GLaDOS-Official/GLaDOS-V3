using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladosV3.Module.ServerBackup.Models
{
    
    public class BackupChannelPerms
    {
        public string PRole { get; set; }
        public ulong AChannelPermissions { get; set; }
        public ulong DChannelPermissions { get; set; }
        public BackupChannelPerms(Overwrite o, SocketGuild g)
        {
            if (g == null) return;
            this.AChannelPermissions = o.Permissions.AllowValue;
            this.DChannelPermissions = o.Permissions.DenyValue;
            this.PRole = g.GetRole(o.TargetId).Name;
        }
    }
}
