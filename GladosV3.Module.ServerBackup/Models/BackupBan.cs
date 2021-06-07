using Discord.Rest;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupBan
    {
        public ulong Id { get; set; }
        public string Reason { get; set; }
        public BackupBan(RestBan b)
        {
            if (b == null) return;
            Id = b.User.Id;
            Reason = b.Reason;
        }
    }
}
