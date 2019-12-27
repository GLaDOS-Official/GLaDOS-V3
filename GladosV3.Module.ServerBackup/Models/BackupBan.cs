using Discord.Rest;

namespace GladosV3.Module.ServerBackup.Models
{
    internal class BackupBan
    {
        public ulong Id { get; set; }
        public string Reason { get; set; }
        public BackupBan(RestBan b)
        {
            Id = b.User.Id;
            Reason = b.Reason;
        }
    }
}
