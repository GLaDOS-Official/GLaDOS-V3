using Discord.Commands;
using System;
using System.Threading.Tasks;
using GladosV3.Helpers;
using Discord;
using Discord.WebSocket;

namespace GladosV3.Module.GTAService
{
    [ServerOnlyAttribute]
    [Name("Meow's services commands")]
    public class MainCommands : ModuleBase<ICommandContext>
    {
        private readonly TokenManager tokenManager;
        private readonly Random rnd = new Random();

        public MainCommands(TokenManager tokenManager) =>  this.tokenManager = tokenManager;

        [Command("service set", RunMode = RunMode.Async)]
        [Remarks("service set <userid> <amount of tokens>")]
        [Summary("Set's users tokens")]
        [GladosV3.Attributes.RequireOwner]
        public async Task GSet(ulong userid, uint tokens)
        {
            var profile = await GtaProfile.Get(userid, Context);
            if (profile == null) { await this.ReplyAsync("No user records found! Creating one..."); profile = await GtaProfile.Create(userid, Context); }
            profile.Tokens = tokens;
            await ReplyAsync("Done!");
        }
        [Command("service info", RunMode = RunMode.Async)]
        [Remarks("service info [userid]")]
        [Summary("Get's users information about services (tokens)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task GInfo(ulong userid = 0)
        {
            userid = userid == 0 ? Context.Message.Author.Id : userid;
            var profile = await GtaProfile.Get(userid, Context);
            if (profile == null) { await ReplyAsync("No user records found!"); return; }
            var builder = new EmbedBuilder()
    .WithTitle("GInfo")
    .WithColor(new Color((uint)this.rnd.Next(0, 0xFFFFFF)))
    .WithCurrentTimestamp()
    .WithFooter(footer => footer
                         .WithText($"Requested by {Context.User.Username}#{Context.User.Discriminator} | ID: {Context.User.Id}")
                         .WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl()))
    .WithThumbnailUrl(profile.AvatarUrl)
    .AddField("Tokens", profile.Tokens, true)
    .AddField("Coupons used", profile.CouponsUsed, true)
    .AddField("Service used", profile.ServiceUsed, true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(
                null,
                embed: embed)
                .ConfigureAwait(false);
        }
        [Command("service gift", RunMode = RunMode.Async)]
        [Remarks("service gift <mention> <tokens>")]
        [Summary("Gifts tokens to another player")]
        public async Task GGift(SocketGuildUser user, uint tokens)
        {
            if(user.Id == Context.Message.Author.Id) { await ReplyAsync("Why would you gift yourself tokens?"); return; }
            var giver = await GtaProfile.Get(Context.Message.Author.Id, Context);
            if (giver == null) { await ReplyAsync("No user records for giver found! Creating one..."); giver = await GtaProfile.Create(Context.Message.Author.Id, Context); }
            if (giver.Tokens < tokens) { await ReplyAsync($"You don't have enough tokens! You currently have {giver.Tokens} tokens"); return; }
            var receiver = await GtaProfile.Get(user.Id, Context);
            if (receiver == null) { await ReplyAsync("No user records for receiver found! Creating one..."); receiver = await GtaProfile.Create(user.Id, Context); }
            giver.Tokens -= tokens;
            receiver.Tokens += tokens;
            await ReplyAsync($"🎁 {user.Mention} You got {tokens} tokens from {Context.Message.Author.Mention}!");
        }

        [Command("service create coupon", RunMode = RunMode.Async)]
        [Remarks("service create coupon <tokens> <maximumUses> [name]")]
        [Summary("Creates a coupon code")]
        [GladosV3.Attributes.RequireOwner]
        public async Task CreateCoupon(uint tokens, uint maximumUses, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name)) name = Tools.RandomString(8);
            await SqLite.Connection.AddRecordAsync("GTA_Coupons", "key,maximumUses,bonusTokens", new object[] { name, maximumUses, tokens });
            await Context.Message.Author.SendMessageAsync($"The coupon code you just generated is `{name}`");
            await ReplyAsync("Done!");
        }
    }
}
