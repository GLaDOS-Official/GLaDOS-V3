using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladosV3.Module.ServerBackup.Models
{
    class BackupBan
    {
        public ulong Id { get; set; }
        public string Reason { get; set; }
        public BackupBan(RestBan b)
        {
            this.Id = b.User.Id;
            this.Reason = b.Reason;
        }
    }
}
