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
        [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireMFA]
        public async Task Purge(int messageCount = 20)
        {
            if (messageCount < 2)
            {
                await this.ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear").ConfigureAwait(false);
                return;
            }
            var count = messageCount + 1;
            var deleted = 0;
            while (count != 0)
            {
                var limit = count < 100 ? count : 100;
                var enumerable = Context.Channel.GetMessagesAsync(limit).Flatten();
                try
                {
                    IOrderedEnumerable<IMessage> messages = (await enumerable.ToArrayAsync().ConfigureAwait(false))
                                                           .Where(msg => (msg.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13)
                                                           .OrderByDescending(msg => msg.Timestamp);
                    if (!messages.Any()) { count = 0; break; }
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages).ConfigureAwait(false);
                    count -= messages.Count();
                    deleted += messages.Count();
                }
                catch (ArgumentOutOfRangeException)
                {
                    await this.ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!").ConfigureAwait(false);
                    return;
                }
            }
            var warning = count != 0 ? "Some messages failed to delete! This is not an error." : null;
            await this.ReplyAsync($"Purged {deleted} messages! {warning}").ConfigureAwait(false);
        }

        [Command("prune")]
        [Remarks("prune <user> [no. of messages]")]
        [Summary("Removes most recent messages from a user")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireMFA]
        public async Task Prune(IUser mention, int messageCount = 20)
        {
            if (messageCount < 2)
            {
                await this.ReplyAsync("**ERROR: **Please Specify the amount of messages you want to clear").ConfigureAwait(false);
            }
            var deleted = 0;
            var count = messageCount;
            while (count != 0)
            {
                var limit = count < 100 ? count : 100;
                var enumerable = Context.Channel.GetMessagesAsync(limit).Flatten();
                try
                {
                    IOrderedEnumerable<IMessage> messages = (await enumerable.ToArrayAsync())
                                                            .Where(x => x.Author == mention && (x.Timestamp - DateTimeOffset.UtcNow).TotalDays > -13)
                                                            .OrderByDescending(msg => msg.Timestamp);
                    if (!messages.Any()) { count = 0; break; }
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages).ConfigureAwait(false);
                    count -= messages.Count();
                    deleted += messages.Count();
                }
                catch (ArgumentOutOfRangeException)
                {
                    await this.ReplyAsync("Some messages failed to delete! This is not a error and can not be fixed!").ConfigureAwait(false);
                    return;
                }
            }
            var warning = count != 0 ? "Some messages failed to delete! This is not an error." : null;
            await this.ReplyAsync($"Purged {deleted} messages! {warning}").ConfigureAwait(false);
        }
        [Command("kick")]
        [Remarks("kick <user> [reason]")]
        [Summary("Kicks the specified user.")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireMFA]
        public async Task Kick(SocketGuildUser mention, [Remainder] string reason = "Unspecified.")
        {
            var silent = false;
            if (reason.Contains("--s", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                reason = reason.Replace("--s", "", StringComparison.OrdinalIgnoreCase);
                silent = true;
            }
            if (mention.Id == Context.User.Id)
            {
                if (!silent)
                    await this.ReplyAsync("Why would you kick yourself?").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you kick yourself?").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (mention.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, you can't kick {mention.Mention} as he's above you.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't kick {mention.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (mention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, I can't kick {mention.Mention} as he's above me.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't kick {mention.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (mention?.Id == Context.Client.CurrentUser.Id)
            { await this.ReplyAsync($"Ok, bye!").ConfigureAwait(false); await Context.Guild.LeaveAsync().ConfigureAwait(false); return; }
            try
            {
                await mention.SendMessageAsync($"You were kicked from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}").ConfigureAwait(false);

                await mention.KickAsync(
                    $"Kicked by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}").ConfigureAwait(false);
                if (!silent)
                    await this.ReplyAsync($"Bai bai {mention.Mention}! :wave:").ConfigureAwait(false);
                else
                    await ((IDMChannel)(await (await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false)).SendMessageAsync($"Bai bai {mention.Mention}! :wave:").ConfigureAwait(false)).Channel).CloseAsync().ConfigureAwait(false);
            }
            catch
            {
                await this.ReplyAsync($"How? I seem that I unable to kick {mention.Mention}!").ConfigureAwait(false);
            }
        }
        [Command("ban")]
        [Remarks("ban <user> [reason]")]
        [Summary("Bans the specified user.")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireMFA]
        public async Task Ban(SocketGuildUser mention, [Remainder] string reason = "Unspecified.")
        {
            var silent = false;
            if (reason.Contains("--s", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                reason = reason.Replace("--s", "", StringComparison.OrdinalIgnoreCase);
                silent = true;
            }
            if (mention?.Id == Context.User.Id)
            {
                if (!silent)
                    await this.ReplyAsync("Why would you ban yourself?").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (mention.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, you can't ban {mention.Mention} as he's above you.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't ban {mention.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (mention?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, I can't ban {mention.Mention} as he's above me.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't ban {mention.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (mention?.Id == Context.Client.CurrentUser.Id)
            { await this.ReplyAsync($"Ok, bye!").ConfigureAwait(false); await Context.Guild.LeaveAsync().ConfigureAwait(false); return; }
            try
            {
                await mention.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}").ConfigureAwait(false);
                await Context.Guild.AddBanAsync(mention, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}").ConfigureAwait(false);
                if (!silent)
                    await this.ReplyAsync($"Begone {mention.Mention}!").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Begone {mention.Mention}!").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);

            }
            catch
            {
                await this.ReplyAsync($"How? I seem that I unable to ban {mention.Mention}!").ConfigureAwait(false);
            }
        }
        [Command("ban")]
        [Remarks("ban <user> [reason]")]
        [Summary("Bans the specified user.")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireMFA]
        public async Task Ban(ulong userid, [Remainder] string reason = "Unspecified.")
        {
            var silent = false;
            if (reason.Contains("--s", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                reason = reason.Replace("--s", "", StringComparison.OrdinalIgnoreCase);
                silent = true;
            }
            if (userid == Context.User.Id)
            {
                if (!silent)
                    await this.ReplyAsync("Why would you ban yourself?").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            SocketGuildUser user = Context.Guild.GetUser(userid);
            SocketGuildUser moderator = Context.User as SocketGuildUser;
            if (user.Hierarchy > moderator?.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, you can't ban {user.Mention} as he's above you.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, you can't ban {user.Mention} as he's above you.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (user?.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                if (!silent)
                    await this.ReplyAsync($"Sorry, I can't ban {user.Mention} as he's above me.").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Sorry, I can't ban {user.Mention} as he's above me.").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            if (user?.Id == Context.Client.CurrentUser.Id)
            { await this.ReplyAsync($"Ok, bye!").ConfigureAwait(false); await Context.Guild.LeaveAsync().ConfigureAwait(false); return; }
            try
            {
                await user.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}").ConfigureAwait(false);
                await Context.Guild.AddBanAsync(user, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}").ConfigureAwait(false);
                if (!silent)
                    await this.ReplyAsync($"Begone {user.Mention}!").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync($"Begone {user.Mention}!").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);

            }
            catch
            {
                await this.ReplyAsync($"How? I seem that I unable to ban {user.Mention}!").ConfigureAwait(false);
            }
        }
        [Command("hackban")]
        [Remarks("hackban <user> [reason]")]
        [Summary("Hackbans the specified user.")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireMFA]
        public async Task Hackban(ulong userid, [Remainder] string reason = "Unspecified.")
        {
            var silent = false;
            if (reason.Contains("--s", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                reason = reason.Replace("--s", "", StringComparison.OrdinalIgnoreCase);
                silent = true;
            }
            if (userid == Context.User.Id)
            {
                if (!silent)
                    await this.ReplyAsync("Why would you ban yourself?").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("Why would you ban yourself?").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }

            SocketGuildUser user = Context.Guild.GetUser(userid);
            if (user != null)
            {
                if (!silent)
                    await this.ReplyAsync("User is in this server!").ConfigureAwait(false);
                else
                    await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync("User is in this server!").GetAwaiter().GetResult().Channel).CloseAsync().ConfigureAwait(false);
                return;
            }
            SocketUser normalUser = Context.Client.GetUser(userid);
            try
            {
                if (normalUser != null)
                    await normalUser.SendMessageAsync($"You were banned from {Context.Guild.Name} for \"{reason}\" by {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.Discriminator}").ConfigureAwait(false);
                await Context.Guild.AddBanAsync(userid, 0,
                    $"Banned by {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} | Reason: {reason}").ConfigureAwait(false);
                if (normalUser != null)
                {
                    if (!silent)
                        await this.ReplyAsync($"Begone {normalUser.Mention}!").ConfigureAwait(false);
                    else
                        await ((IDMChannel)Context.Message.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult()
                                .SendMessageAsync($"Begone {normalUser.Mention}!").GetAwaiter().GetResult().Channel)
                            .CloseAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                await this.ReplyAsync($"How? I seem that I unable to ban {normalUser?.Mention}!").ConfigureAwait(false);
            }
        }

        [Command("say"), Alias("s")]
        [Remarks("say <text>")]
        [Summary("Make the bot say something")]
        [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
        public async Task Say([Remainder] string text)  
        {
            if (text.Contains("--s", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                text = text.Replace("--s", "", StringComparison.Ordinal);
            }
            await this.ReplyAsync(text).ConfigureAwait(false);
        }
    }
}
