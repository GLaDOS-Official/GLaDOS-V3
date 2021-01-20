using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GladosV3;
using GladosV3.Helpers;
using Interactivity;
using Interactivity.Confirmation;
using Interactivity.Pagination;
using Interactivity.Selection;
using ConnectionState = System.Data.ConnectionState;

namespace GladosV3.Module.GTAService
{
    public class TokenManager
    {
        public static TokenManager service;
        private SocketTextChannel mainChannel;
        private RestUserMessage mainMessage;
        private IEmote gtaOnlineEmote;
        private IEmote redeemCouponEmote;
        private InteractivityService Interactivity;
        public TokenManager(InteractivityService service, DiscordSocketClient discord) => this.Initialize(service, discord);

        [DllImport("apiaccess.dll", EntryPoint = "DoStuff", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern byte GiveMoney(string email, string password, string username, ulong money);
        public async Task Initialize(InteractivityService s, DiscordSocketClient client)
        {
            while (SqLite.Connection.State != ConnectionState.Open) await Task.Delay(1000);
            if (!await SqLite.Connection.TableExistsAsync("GTA_Tokens")) await SqLite.Connection.CreateTableAsync("GTA_Tokens", "`userId` INTEGER NOT NULL UNIQUE, `tokens` INTEGER NOT NULL DEFAULT 0, `couponsUsed` INTEGER NOT NULL DEFAULT 0, `serviceUsed` INTEGER NOT NULL DEFAULT 0, `userBlacklisted` TEXT");
            if (!await SqLite.Connection.TableExistsAsync("GTA_Coupons")) await SqLite.Connection.CreateTableAsync("GTA_Coupons", "`key` TEXT UNIQUE, `maximumUses` INTEGER, `uses` INTEGER NOT NULL DEFAULT 0, `bonusTokens` INTEGER NOT NULL");
            this.Interactivity = s;
            client.UserJoined += this.DiscordOnUserJoined;
            client.MessageReceived += this.DiscordOnOrderReceive;
            await client.WaitForConnection();
            this.mainChannel = ((SocketTextChannel)client.GetGuild(783348387105865778).GetChannel(784410131378602004));
            this.mainMessage = (RestUserMessage)await this.mainChannel.GetMessageAsync(784417643867668530);
            this.gtaOnlineEmote = Emote.Parse("<:GTAO:784505757437657158>");
            this.redeemCouponEmote = new Emoji("\U0001F3AB");
            if (!this.mainMessage.Reactions.ContainsKey(this.gtaOnlineEmote)) await this.mainMessage.AddReactionAsync(this.gtaOnlineEmote);
            if (!this.mainMessage.Reactions.ContainsKey(this.redeemCouponEmote)) await this.mainMessage.AddReactionAsync(this.redeemCouponEmote);
            client.ReactionAdded += this.ReactionAdded;
        }

        private async Task gtaOnlineMoneyRoutine(IUser user)
        {

            // TODO: Finish this
            var dm = await user.GetOrCreateDMChannelAsync();
            this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync("Hello! Thank you again for using our service! Now please tell me how many tokens you wanna spend?"), TimeSpan.FromMinutes(3));

            //tokens
            var result = await this.Interactivity.NextMessageAsync(x => x.Author.Id == user.Id && dm.Id == x.Channel.Id, timeout: TimeSpan.FromMinutes(2));
            if (!result.IsSuccess) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("You did not respond within time limit!"))); return; }
            if (string.IsNullOrWhiteSpace((result.Value.Content))) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("No"))); return; }
            if (!uint.TryParse(result.Value.Content, out var tokens))
            {
                this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync("Token must be a number! You failed... :("), TimeSpan.FromMinutes(1));
                return;
            }

            var profile = await GtaProfile.Create(user.Id, null);
            if(profile.Tokens < tokens)
            {
                this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync($"Sorry, but you don't have that many tokens! You currently have {profile.Tokens} tokens!"), TimeSpan.FromMinutes(1));
                return;
            }
            this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync("Thanks! Now please tell me your email address you use to log into GTA Online."), TimeSpan.FromMinutes(3));
            //email
            result = await this.Interactivity.NextMessageAsync(x => x.Author.Id == user.Id && dm.Id == x.Channel.Id, timeout: TimeSpan.FromMinutes(2));
            if (!result.IsSuccess) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("You did not respond within time limit!"))); return; }
            if (string.IsNullOrWhiteSpace((result.Value.Content))) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("No"))); return; }
            string email = result.Value.Content;
            this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync("Thanks! Now please tell me your password you use to log into GTA Online."), TimeSpan.FromMinutes(3));
            //password
            result = await this.Interactivity.NextMessageAsync(x => x.Author.Id == user.Id && dm.Id == x.Channel.Id, timeout: TimeSpan.FromMinutes(2));
            if (!result.IsSuccess) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("You did not respond within time limit!"))); return; }
            if (string.IsNullOrWhiteSpace((result.Value.Content))) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("No"))); return; }
            string password = result.Value.Content;
            this.Interactivity.DelayedDeleteMessageAsync(await dm.SendMessageAsync("Thanks! Now please tell me your username in GTA Online."), TimeSpan.FromMinutes(3));
            //username
            result = await this.Interactivity.NextMessageAsync(x => x.Author.Id == user.Id && dm.Id == x.Channel.Id, timeout: TimeSpan.FromMinutes(2));
            if (!result.IsSuccess) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("You did not respond within time limit!"))); return; }
            if (string.IsNullOrWhiteSpace((result.Value.Content))) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("No"))); return; }
            string username = result.Value.Content;
            this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("Working!!! Please wait...")));
            ulong money = tokens * 8000000;
            var code = GiveMoney(email, password, username, money);
            switch (Convert.ToInt16(code))
            {
                case 0:
                    profile.Tokens -= tokens;
                    this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync($"Successfully added {money} into your bank! Thanks for using our services!")));
                    return;
                case 1:
                    this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("Error! Invalid credentials maybe?")));
                    return;
                case 2:
                    this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("Error! Maybe your account is banned?")));
                    return;
            }
            try { }
            finally { await dm.CloseAsync(); } // epic hack
            //this.interactiveService.NextMessageAsync
        }
        private async Task redeemCouponRoutine(IUser user)
        {

            // TODO: Finish this
            var dm = await user.GetOrCreateDMChannelAsync();
            this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("Hello! Thank you again for using our service! Now please tell me coupon code you wanna use?")), TimeSpan.FromMinutes(2));
            var result = await this.Interactivity.NextMessageAsync(x => x.Author.Id == user.Id && dm.Id == x.Channel.Id, timeout: TimeSpan.FromMinutes(2));
            if (!result.IsSuccess) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("You did not respond within time limit!"))); return; }
            if (string.IsNullOrWhiteSpace((result.Value.Content))) { this.Interactivity.DelayedDeleteMessageAsync((await dm.SendMessageAsync("No"))); return; }

            DataTable dt = await SqLite.Connection.GetValuesAsync("GTA_Coupons", $"WHERE key='{HttpUtility.HtmlEncode(result.Value.Content)}'");
            if (dt.Rows.Count == 0)
            {
                this.Interactivity
                    .DelayedDeleteMessageAsync((await dm.SendMessageAsync("Sorry, but this coupon code is either expired or invalid!")),
                                               TimeSpan.FromMinutes(1));
            }

            var tokens = uint.Parse(dt.Rows[0][3].ToString());
            var profile = await GtaProfile.Create(user.Id, null);
            profile.Tokens += tokens;
            profile.CouponsUsed++;
            this.Interactivity
                .DelayedDeleteMessageAsync((await dm.SendMessageAsync($"Congratulations! You redeem {tokens} tokens!")),
                                           TimeSpan.FromMinutes(1));
            //TODO: update the uses and check or delete the coupon
            var uses = uint.Parse(dt.Rows[0][2].ToString()) + 1;
            if (uint.Parse(dt.Rows[0][1].ToString()) <= uses)
                await SqLite.Connection.RemoveRecordAsync("GTA_Coupons",
                                                          $"key='{HttpUtility.HtmlEncode(result.Value.Content)}'");
            else await SqLite.Connection.SetValueAsync("GTA_Coupons", "uses", uses);
            try { }
            finally { await dm.CloseAsync(); } //epic hack
            //this.interactiveService.NextMessageAsync
        }
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel.Id != 784410131378602004) return;
            if (reaction.User.Value.IsBot || reaction.User.Value.IsWebhook) return;
            this.mainMessage.RemoveReactionAsync(reaction.Emote, reaction.UserId);
            if (Equals(reaction.Emote, this.gtaOnlineEmote)) { this.gtaOnlineMoneyRoutine(reaction.User.Value); }
            else if (Equals(reaction.Emote, this.redeemCouponEmote)) { this.redeemCouponRoutine(reaction.User.Value); }

        }

        private async Task DiscordOnOrderReceive(SocketMessage message)
        {
            if (message.Channel.Id != 783978483087835177) return;
            if (message.Author.Id != 439717362271518722) return;
            var channel = (message.Channel as SocketTextChannel);
            var status = message.Embeds.First().Title;
            if (status != "Order Completed") await (channel.Guild.GetChannel(783980828177989653) as SocketTextChannel).SendMessageAsync($"Something went wrong! https://discord.com/channels/{channel.Guild.Id}/{channel.Id}/{message.Id}");
            var tokens = uint.Parse(message.Embeds.First().Fields[0].Value.Split(' ')[0]);
            var userId = ulong.Parse(message.Embeds.First().Fields.Last().Value);
            var profile = await GtaProfile.Get(userId, null);
            profile.Tokens += tokens;
            await (channel.Guild.GetChannel(783348387532767254) as SocketTextChannel).SendMessageAsync($"Somebody bought {tokens} tokens! Thank you! ❤️");
        }

        private async Task DiscordOnUserJoined(SocketGuildUser arg)
        {
            if (arg.IsBot) return;
            if (arg.Guild.Id != 783348387105865778) return;
            await GtaProfile.Create(arg.Id, null);
        }

    }
}
