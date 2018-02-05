using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using GladosV3.Helpers;
using GladosV3.Attributes;
namespace GladosV3.Modules
{
    [Name("Moderator")]
    [RequireContext(ContextType.Guild)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Remarks("purge [no. of messages]")]
        [Summary("Removes specified amount of messages")]
        [Attributes.RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Attributes.RequireMFA]
        public async Task Purge(int count = 100)
        {
            if (count < 2)
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            else if (count > 100)
                await ReplyAsync("**Error: **I can only clear 100 Messages at a time!");
            else
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                var limit = count < 100 ? count : 100;
                var enumerable = await Context.Channel.GetMessagesAsync(limit).Flatten().ConfigureAwait(false);
                try
                {
                    await Context.Channel.DeleteMessagesAsync(enumerable).ConfigureAwait(false);
                    await ReplyAsync($"Purged {enumerable.Count().ToString()} messages!");
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
                    return;
                }
            }
        }

        [Command("prune")]
        [Remarks("prune <user> [no. of messages]")]
        [Summary("Removes most recent messages from a user")]
        [Attributes.RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Attributes.RequireMFA]
        public async Task Prune(IUser UserMention, int count = 100)
        {
            if (count < 2)
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            else if (count > 100)
                await ReplyAsync("**Error: **I can only clear 100 Messages at a time!");
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var newlist = (await Context.Channel.GetMessagesAsync().Flatten().ConfigureAwait(false)).Where(x => x.Author == UserMention).Take(count).ToArray();
            try
            {
                await Context.Channel.DeleteMessagesAsync(newlist).ConfigureAwait(false);
                await ReplyAsync($"Purged {newlist.Count().ToString()} from <@{UserMention.Id}> messages!");
            }
            catch (ArgumentOutOfRangeException)
            {
                await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
                return;
            }
            await ReplyAsync($"Cleared **{UserMention.Username}'s** Messages (Count = {newlist.Length.ToString()})");
        }
        [Command("kick")]
        [Remarks("kick <user> [reason]")]
        [Summary("Kicks the specified user.")]
        [Attributes.RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [Attributes.RequireMFA]
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
        [Remarks("ban <user> [reason]")]
        [Summary("Bans the specified user.")]
        [Attributes.RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Attributes.RequireMFA]
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
        [Remarks("leave")]
        [Summary("Sad to see you go!")]
        [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in a server."); return; }
            await ReplyAsync("Bye! :wave:");
            await Context.Guild.LeaveAsync();
        }

        [Command("say"), Alias("s")]
        [Remarks("say <text>")]
        [Summary("Make the bot say something")]
        [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Say([Remainder] string text)
        {
            if (text.Contains("--s"))
            {
                await Context.Message.DeleteAsync();
                text = text.Replace("--s", "");
            }
            await ReplyAsync(text);
        }
    }
}
