using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using GladosV3.Helpers;

namespace GladosV3.Modules
{
    [Name("Moderator")]
    [RequireContext(ContextType.Guild)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Summary("purge <no. of messages>")]
        [Remarks("Removes specified amount of messages")]
        public async Task Prune(int count = 100)
        {
            if (count < 1)
            {
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            }
            else if (count > 100)
            {
                await ReplyAsync("**Error: **I can only clear 100 Messages at a time!");
            }
            else
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                var limit = count < 100 ? count : 100;
                var enumerable = await Context.Channel.GetMessagesAsync(limit).Flatten().ConfigureAwait(false);
                await Context.Channel.DeleteMessagesAsync(enumerable).ConfigureAwait(false);
                await ReplyAsync($"Cleared **{count}** Messages");
            }
        }

        [Command("prune")]
        [Summary("prune <user>")]
        [Remarks("Removes most recent messages from a user")]
        public async Task Prune(IUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var enumerable = await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false);
            var newlist = enumerable.Where(x => x.Author == user).ToList();
            await Context.Channel.DeleteMessagesAsync(newlist).ConfigureAwait(false);
            await ReplyAsync($"Cleared **{user.Username}'s** Messages (Count = {newlist.Count})");
        }
        [Command("kick")]
        [Summary("kick <user> [reason]")]
        [Remarks("Kick the specified user.")]
        [Helpers.RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(SocketGuildUser UserMention, [Remainder] string reason = "Unspecified.")
        {
            if (UserMention.Id == Context.User.Id)
            { await ReplyAsync("Why would you kick yourself?"); return; }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (UserMention.Hierarchy > moderator?.Hierarchy)
            { await ReplyAsync($"Sorry, you can't kick {UserMention.Mention} as he's above you."); return; }
            else if (UserMention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            { await ReplyAsync($"Sorry, I can't kick {UserMention.Mention} as he's above me."); return; }
            else if (UserMention?.Id == Context.Client.CurrentUser.Id)
            { await ReplyAsync($"Ok, bye!"); await Context.Guild.LeaveAsync(); return; }
            try
            {
                await UserMention.KickAsync(
                    $"Kicked by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator} Reason: {reason}");
                await ReplyAsync($"Bai bai {UserMention.Mention}! :wave:");
            }
            catch
            {
                await ReplyAsync($"How? I seem that I unable to kick {UserMention.Mention}!");
            }
        }
        [Command("ban")]
        [Summary("ban <user> [reason]")]
        [Remarks("Bans the specified user.")]
        [Helpers.RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(SocketGuildUser UserMention, [Remainder] params string[] reasonArray)
        {
            if (UserMention.Id == Context.User.Id)
            { await ReplyAsync("Why would you ban yourself?"); return; }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if(moderator?.Hierarchy > UserMention.Hierarchy)
            { await ReplyAsync($"Sorry, you can't ban {UserMention.Mention} as he's above you."); return;}
            else if(UserMention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            { await ReplyAsync($"Sorry, I can't ban {UserMention.Mention} as he's above me."); return; }
            else if(UserMention?.Id == Context.Client.CurrentUser.Id)
            { await ReplyAsync($"Ok, bye!"); await Context.Guild.LeaveAsync();  return; }
            try
            {
                
                await Context.Guild.AddBanAsync(UserMention, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} Reason: {string.Join(" ", reasonArray)}");
                await ReplyAsync($"Begone {UserMention.Mention}!");
            }
            catch
            {
                await ReplyAsync($"How? I seem that I unable to ban {UserMention.Mention}!");
            }
        }
        [Command("leave")]
        [Summary("leave")]
        [Remarks("Sad to see you go!")]
        [Helpers.RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in a server."); return; }
            await ReplyAsync("Bye! :wave:");
            await Context.Guild.LeaveAsync();
        }
        [Command("say"), Alias("s")]
        [Summary("say <text>")]
        [Remarks("Make the bot say something")]
        [Helpers.RequireUserPermission(GuildPermission.ManageGuild)]
        public Task Say([Remainder]string text)
            => ReplyAsync(text);
    }
}
