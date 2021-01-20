using Discord.Commands;
using GladosV3.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.GTAService
{
    public class GtaProfile
    {
        private uint _serviceUsed;
        private uint _tokens;
        private uint _couponsUsed;
        public ulong Userid { get; }
        public string AvatarUrl { get;}

        public uint Tokens
        {
            get => this._tokens;
            set
            {
                if (value == this._tokens) return;
                SqLite.Connection.SetValueAsync("GTA_Tokens", "tokens", value, $"WHERE userId='{Userid}'");
                this._tokens = value;
            }
        }

        public uint CouponsUsed
        {
            get => this._couponsUsed;
            set
            {
                if (value == this._couponsUsed) return;
                SqLite.Connection.SetValueAsync("GTA_Tokens", "couponsUsed", value, $"WHERE userId='{Userid}'");
                this._couponsUsed = value;
            }
        }
        public uint ServiceUsed
        {
            get => this._serviceUsed; set
            {
                if (value == ServiceUsed) return;
                SqLite.Connection.SetValueAsync("GTA_Tokens", "serviceUsed", value, $"WHERE userId='{Userid}'");
                this._serviceUsed = value;
            }
        }
        private GtaProfile(ulong userid, DataRow profileUser, ICommandContext Context)
        {
            this._tokens = uint.Parse(profileUser["tokens"].ToString());
            this._couponsUsed = uint.Parse(profileUser["couponsUsed"].ToString());
            this._serviceUsed = uint.Parse(profileUser["serviceUsed"].ToString());
            this.Userid = userid;
            if (Context == null) return;
            this.AvatarUrl = Context.Message.Author.GetAvatarUrl();
            if (Context.Guild == null) return;
            var user = (Context.Guild.GetUserAsync(userid).GetAwaiter().GetResult());
            this.AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }
        public static async Task<GtaProfile> Get(ulong userid, ICommandContext Context)
        {
            var db = await SqLite.Connection.GetValuesAsync("GTA_Tokens", $"WHERE userId='{userid}'");
            return db.Rows.Count == 0 ? null : new GtaProfile(userid, db.Rows[0], Context);
        }
        public static async Task<GtaProfile> Create(ulong userid, ICommandContext Context)
        {
            if(!(await SqLite.Connection.RecordExistsAsync("GTA_Tokens", $"WHERE userId='{userid}'")))
            await SqLite.Connection.AddRecordAsync("GTA_Tokens", "userId,tokens,couponsUsed,serviceUsed", new[] { userid, (ulong)0, (ulong)0, (ulong)0 });
            return await Get(userid, Context);
        }
    }
}
