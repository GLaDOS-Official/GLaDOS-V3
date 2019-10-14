using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using System;
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
        public async Task Purge(int count = 20)
        {
            if (count < 2)
            {
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
                return;
            }
            int deleted = 0;
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            while (deleted != count)
            {
                int limit = count < 100 ? count : 100;
                var enumerable = Context.Channel.GetMessagesAsync(limit).Flatten();
                try
                {
                    IMessage[] enumerable1 = await enumerable.ToArray();
                    IOrderedEnumerable<IMessage> messages = enumerable1
                        .Where((something) => (something.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13)
                        .OrderByDescending(msg => msg.Timestamp);
                    await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
                    deleted += messages.Count();
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
                    return;
                }
            }
            string warning = count - deleted != 0 ? "Some messages failed to delete! This is not an error." : null;
            await ReplyAsync($"Purged {deleted} messages! {warning}");
        }

        [Command("prune")]
        [Remarks("prune <user> [no. of messages]")]
        [Summary("Removes most recent messages from a user")]
        [Attributes.RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireMFA]
        public async Task Prune(IUser UserMention, int count = 20)
        {
            if (count < 2)
            {
                await ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear");
            }
            int deleted = 0;
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            while (deleted != count)
            {
                int limit = count < 100 ? count : 100;
                IMessage[] newlist = await (Context.Channel.GetMessagesAsync().Flatten()).Where(x => x.Author == UserMention && (x.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13).Take(count).ToArray();
                try
                {
                    IMessage[] enumerable1 = newlist.ToArray();
                    IOrderedEnumerable<IMessage> messages = enumerable1
                        .Where((something) => (something.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13)
                        .OrderByDescending(msg => msg.Timestamp);
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                    deleted += messages.Count();
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!");
                    return;
                }
            }
            await ReplyAsync($"Purged {deleted} from <@{UserMention.Id}> messages!");
            if (deleted > 0)
            {
                await ReplyAsync($"Cleared **{UserMention.Username}'s** Messages (Count = {deleted})");
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
                    $"Kicked by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}");
                if (!silent)
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
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}");
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
        [Command("ban")]
        [Remarks("ban <user> [reason]")]
        [Summary("Bans the specified user.")]
        [Attributes.RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireMFA]
        public async Task Ban(ulong userid, [Remainder] string reason = "Unspecified.")
        {
            bool silent = false;
            if (reason.Contains("--s"))
            {
                await Context.Message.DeleteAsync();
                reason = reason.Replace("--s", "");
                silent = true;
            }
            if (userid == Context.User.Id)
            {
                if (!silent)
                    await ReplyAsync("Why would you ban yourself?");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            SocketGuildUser user = Context.Guild.GetUser(userid);
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (user.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, you can't ban {user.Mention} as he's above you.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't ban {user.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (user?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await ReplyAsync($"Sorry, I can't ban {user.Mention} as he's above me.");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't ban {user.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            if (user?.Id == Context.Client.CurrentUser.Id)
            { await ReplyAsync($"Ok, bye!"); await Context.Guild.LeaveAsync(); return; }
            try
            {
                await user.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}");
                await Context.Guild.AddBanAsync(user, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}");
                if (!silent)
                    await ReplyAsync($"Begone {user.Mention}!");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Begone {user.Mention}!").GetAwaiter().GetResult().Channel).CloseAsync();

            }
            catch
            {
                await ReplyAsync($"How? I seem that I unable to ban {user.Mention}!");
            }
        }
        [Command("hackban")]
        [Remarks("hackban <user> [reason]")]
        [Summary("Hackbans the specified user.")]
        [Attributes.RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireMFA]
        public async Task Hackban(ulong userid, [Remainder] string reason = "Unspecified.")
        {
            bool silent = false;
            if (reason.Contains("--s"))
            {
                await Context.Message.DeleteAsync();
                reason = reason.Replace("--s", "");
                silent = true;
            }
            if (userid == Context.User.Id)
            {
                if (!silent)
                    await ReplyAsync("Why would you ban yourself?");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }

            SocketGuildUser user = Context.Guild.GetUser(userid);
            if (user != null)
            {
                if (!silent)
                    await ReplyAsync("User is in this server!");
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("User is in this server!").GetAwaiter().GetResult().Channel).CloseAsync();
                return;
            }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            SocketUser normaluser = Context.Client.GetUser(userid);
            try
            {
                if (normaluser != null)
                    await normaluser.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}");
                await Context.Guild.AddBanAsync(userid, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}");
                if (normaluser != null)
                {
                    if (!silent)
                        await ReplyAsync($"Begone {normaluser.Mention}!");
                    else
                        await ((IDMChannel) Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult()
                                .SendMessageAsync($"Begone {normaluser.Mention}!").GetAwaiter().GetResult().Channel)
                            .CloseAsync();
                }
            }
            catch
            {
                await ReplyAsync($"How? I seem that I unable to ban {normaluser.Mention}!");
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
