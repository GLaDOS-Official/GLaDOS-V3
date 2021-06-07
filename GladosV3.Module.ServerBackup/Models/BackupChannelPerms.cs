using Discord;
using Discord.WebSocket;

namespace GLaDOSV3.Module.ServerBackup.Models
{

    public class BackupChannelPerms
    {
        public string RoleName { get; set; }
        public ulong AllowPermissions { get; set; }
        public ulong DenyPermissions { get; set; }
        public BackupChannelPerms(Overwrite o, SocketGuild g)
        {
            if (g == null) return;
            AllowPermissions = o.Permissions.AllowValue;
            DenyPermissions = o.Permissions.DenyValue;
            RoleName = g.GetRole(o.TargetId).Name;
        }
    }
}
