using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladosV3.Module.ServerBackup.Models
{
    
    class BackupRole
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
            this.RoleName = r.Name;
            this.RoleMembers = r.Members.Select(m => m.Id).ToList();
            this.RawColour = r.Color.RawValue;
            this.GuildPermissions = r.Permissions.RawValue;
            this.Position = r.Position;
            this.Hoisted = r.IsHoisted;
            this.AllowMention = r.IsMentionable;
        }
    }
}
