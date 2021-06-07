using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupRole
    {
        public string RoleName { get; set; }
        public List<ulong> RoleMembers { get; set; }
        public uint RawColour { get; set; }
        public ulong GuildPermissions { get; set; }
        public int Position { get; set; }
        public bool Hoisted { get; set; }
        public bool AllowMention { get; set; }
        public BackupRole(SocketRole r)
        {
            if (r == null) return;
            RoleName = r.Name;
            RoleMembers = r.Members.Select(m => m.Id).ToList();
            RawColour = r.Color.RawValue;
            GuildPermissions = r.Permissions.RawValue;
            Position = r.Position;
            Hoisted = r.IsHoisted;
            AllowMention = r.IsMentionable;
        }
    }
}
