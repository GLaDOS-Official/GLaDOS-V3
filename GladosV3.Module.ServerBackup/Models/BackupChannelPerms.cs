using Discord;
using Discord.WebSocket;

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
            AChannelPermissions = o.Permissions.AllowValue;
            DChannelPermissions = o.Permissions.DenyValue;
            PRole = g.GetRole(o.TargetId).Name;
        }
    }
}
