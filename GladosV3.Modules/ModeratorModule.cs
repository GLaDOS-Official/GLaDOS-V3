using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Module.Default
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
        [RequireMFA]
        public async Task Purge(int count = 100)
        {
            if (count < 2)
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
                int limit = count < 100 ? count : 100;
                var enumerable = Context.Channel.GetMessagesAsync(limit).Flatten();
                try
                {
                    IMessage[] enumerable1 = await enumerable.ToArray();
                    IOrderedEnumerable<IMessage> messages = enumerable1.Where((something) => (something.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13).OrderByDescending(msg => msg.Timestamp);
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                    string warning = enumerable1.Count() - messages.Count() != 0 ? "Some messages failed to delete! This is not an error." : null;
                    await ReplyAsync($"Purged {messages.Count().ToString()} messages! {warning}");
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
                }
            }
        }

        [Command("prune")]
        [Remarks("prune <user> [no. of messages]")]
        [Summary("Removes most recent messages from a user")]
        [Attributes.RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireMFA]
        public async Task Prune(IUser UserMention, int count = 100)
        {
            if (count < 2)
            {
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            }
            else if (count > 100)
            {
                await ReplyAsync("**Error: **I can only clear 100 Messages at a time!");
            }

            await Context.Message.DeleteAsync().ConfigureAwait(false);
            IMessage[] newlist = await (Context.Channel.GetMessagesAsync().Flatten()).Where(x => x.Author == UserMention && (x.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13).Take(count).ToArray();
            try
            {
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(newlist).ConfigureAwait(false);
                await ReplyAsync($"Purged {newlist.Count().ToString()} from <@{UserMention.Id}> messages!");
            }
            catch (ArgumentOutOfRangeException)
            {
                await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
            }
            if (newlist.Length > 0)
            {
                await ReplyAsync($"Cleared **{UserMention.Username}'s** Messages (Count = {newlist.Length.ToString()})");
            }
        }
        [Command("kick")]
        [Remarks("kick <user> [reason]")]
        [Summary("Kicks the specified user.")]
        [Attributes.RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireMFA]
        public async Task Kick(SocketGuildUser UserMention, [Remainder] string reason = "Unspecified.")
        {
            bool silent = false;
            if (reason.Contains("--s"))
            {
                await Context.Message.DeleteAsync();
                reason = reason.Replace("--s", "");
                silent = true;
            }
            if (UserMention.Id == Context.User.Id)
            {
                if (!silent)
                    await ReplyAsync("Why would you kick yourself?");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you kick yourself?").GetAwaiter().GetResult().Channel).CloseAsync();
               return;
            }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (UserMention.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, you can't kick {UserMention.Mention} as he's above you.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't kick {UserMention.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (UserMention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, I can't kick {UserMention.Mention} as he's above me.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't kick {UserMention.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (UserMention?.Id == Context.Client.CurrentUser.Id)
            { await ReplyAsync($"Ok, bye!"); await Context.Guild.LeaveAsync(); return; }
            try
            {
                await UserMention.SendMessageAsync($"You were kicked from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}");

                await UserMention.KickAsync(
                    $"Kicked by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} Reason: {reason}");
                if(!silent)
                    await ReplyAsync($"Bai bai {UserMention.Mention}! :wave:");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Bai bai {UserMention.Mention}! :wave:").GetAwaiter().GetResult().Channel).CloseAsync();
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
        [RequireMFA]
        public async Task Ban(SocketGuildUser UserMention, [Remainder] string reason = "Unspecified.")
        {
            bool silent = false;
            if (reason.Contains("--s"))
            {
                await Context.Message.DeleteAsync();
                reason = reason.Replace("--s", "");
                silent = true;
            }
            if (UserMention.Id == Context.User.Id)
            {
                if (!silent)
                    await ReplyAsync("Why would you ban yourself?");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (UserMention.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, you can't ban {UserMention.Mention} as he's above you.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't ban {UserMention.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (UserMention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, I can't ban {UserMention.Mention} as he's above me.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't ban {UserMention.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (UserMention?.Id == Context.Client.CurrentUser.Id)
            { await ReplyAsync($"Ok, bye!"); await Context.Guild.LeaveAsync(); return; }
            try
            {
                await UserMention.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}");
                await Context.Guild.AddBanAsync(UserMention, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} Reason: {reason}");
                if (!silent)
                    await ReplyAsync($"Begone {UserMention.Mention}!");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Begone {UserMention.Mention}!").GetAwaiter().GetResult().Channel).CloseAsync();

            }
            catch
            {
                await ReplyAsync($"How? I seem that I unable to ban {UserMention.Mention}!");
            }
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
