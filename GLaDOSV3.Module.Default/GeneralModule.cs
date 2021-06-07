using Discord;
using Discord.Commands;
using GLaDOSV3.Attributes;
using GLaDOSV3.Helpers;
using GLaDOSV3.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.Default
{
    [Name("General")]
    [RequireContext(ContextType.Guild)]
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private static MemoryCache mCache;
        public GeneralModule() { mCache ??= new MemoryCache(new MemoryCacheOptions()); }
        [Name("Guild settings")]
        public class Guild : ModuleBase<SocketCommandContext>
        {
            [Command("guild farewell message")]
            [Summary("Set the current message of Guild Join module")]
            [Remarks("guild join message <message>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task FarewellMessage(string value)
            {
                await SqLite.Connection.SetValueAsync("servers", "leave_msg", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild farewell channel")]
            [Summary("Set the current channel ID of Guild Join module")]
            [Remarks("guild join channel <channelId>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task FarewellChannel(string value)
            {
                if (Context.Guild.GetChannel(Convert.ToUInt64(value, CultureInfo.InvariantCulture)) != null)
                    await SqLite.Connection.SetValueAsync("servers", "joinleave_cid", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                else
                    throw new Exception("Channel ID is invalid!");

                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild farewell status")]
            [Summary("Set the current status of Guild Join module")]
            [Remarks("guild join status <status>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task FarewellStatus(string value)
            {
                if (value == "1" || value == "0")
                    await SqLite.Connection.SetValueAsync("servers", "leave_toggle", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                else
                    throw new Exception("Only 0 or 1 is accepted!");
                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild join message")]
            [Summary("Set the current message of Guild Join module")]
            [Remarks("guild join message <message>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task JoinMessage(string value)
            {
                await SqLite.Connection.SetValueAsync("servers", "join_msg", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild join channel")]
            [Summary("Set the current channel ID of Guild Join module")]
            [Remarks("guild join channel <channelId>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task JoinChannel(string value)
            {
                if (!ulong.TryParse(value, out var id)) throw new Exception("Channel ID is invalid!");
                if (Context.Guild.GetChannel(id) != null)
                    await SqLite.Connection.SetValueAsync("servers", "joinleave_cid", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                else
                    throw new Exception("Channel ID is invalid!");

                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild join status")]
            [Summary("Set the current status of Guild Join module")]
            [Remarks("guild join status <status>")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task JoinStatus(string value)
            {
                if (value == "1" || value == "0")
                    await SqLite.Connection.SetValueAsync("servers", "join_toggle", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                else
                    throw new Exception("Only 0 or 1 is accepted!");
                await this.ReplyAsync("Done!").ConfigureAwait(false);
            }
            [Command("guild prefix")]
            [Summary("Set the guild prefix of this bot")]
            [Remarks("guild prefix")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task GuildPrefix(string value = null)
            {
                await SqLite.Connection.SetValueAsync("servers", "prefix", value, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                if (CommandHandler.Prefix.ContainsKey(Context.Guild.Id))
                    CommandHandler.Prefix.Remove(Context.Guild.Id);
                if (!string.IsNullOrWhiteSpace(value))
                    CommandHandler.Prefix.Add(Context.Guild.Id, value);
                await this.ReplyAsync($"Done! Changed the prefix to: {(string.IsNullOrWhiteSpace(value) ? IsOwner.botSettingsHelper["prefix"] : value)}").ConfigureAwait(false);
            }
            [Command("guild configuration")]
            [Summary("Lists the current settings of the Guild module")]
            [Remarks("guild configuration")]
            [Alias("guild config")]
            [Attributes.RequireUserPermissionAttribute(GuildPermission.ManageGuild)]
            public async Task GuildConfig()
            {
                var msg = await this.ReplyAsync("Please wait...").ConfigureAwait(true);
                var finalMsg = string.Empty;
                DataTable dt = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}'").ConfigureAwait(true);
                var row = dt.Rows[0];
                Random rnd = new Random();
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                finalMsg = $"NSFW module status: {(row?[1] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
                finalMsg += $"Join and leave announcement channel: {(string.IsNullOrWhiteSpace(row?[2].ToString()) ? "Not set ❌" : $"<#{row?[2]}> ✅")}\n";
                finalMsg += $"Join message: {row?[3]}\n";
                finalMsg += $"Join announcement status: {(row?[4] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
                finalMsg += $"Leave message: {row?[5]}\n";
                finalMsg += $"Leave announcement status: {(row?[6] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                finalMsg += $"Guild prefix: {(string.IsNullOrWhiteSpace(row?[7].ToString()) ? IsOwner.botSettingsHelper["prefix"] : row?[7])}";
                builder.AddField("Guild settings", finalMsg);
                await msg.ModifyAsync((a) => { a.Content = string.Empty; a.Embed = builder.Build(); }).ConfigureAwait(false);
            }
        }
        [Command("bot rebuild all")]
        [Remarks("bot rebuild all")]
        [Summary("Rebuilds the DB completely, bot will be unavailable while rebuilding")]
        [Attributes.RequireOwner]
        public async Task RebuildDB_All(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                mCache.Set("rebuildDB_All", random, TimeSpan.FromSeconds(45));
                await this.ReplyAsync($"This is a dangerous operation! All server settings, blacklisted users, blacklisted servers will be lost! Type the \"{random}\" to rebuild it! This key will expire in 45 seconds!").ConfigureAwait(false);
                return;
            }
            if (!mCache.TryGetValue("rebuildDB_All", out string entry))
            { await this.ReplyAsync("Key expired or not created!").ConfigureAwait(false); return; }
            if (entry != key)
            { await this.ReplyAsync("Incorrect key!").ConfigureAwait(false); return; }
            mCache.Remove("rebuildDB_All");
            await SqLite.Connection.ExecuteSQL("DROP TABLE servers").ConfigureAwait(false);
            await SqLite.Connection.ExecuteSQL("DROP TABLE BlacklistedUsers").ConfigureAwait(false);
            await SqLite.Connection.ExecuteSQL("DROP TABLE BlacklistedServers").ConfigureAwait(false);
            SqLite.Connection.Close();
            SqLite.Start();
            var message = await this.ReplyAsync("Bot will be unavailable for a while. Rebuilding the database.....\nRefactoring server table...").ConfigureAwait(true);
            CommandHandler.BotBusy = true;
            for (var i = 0; i < Context.Client.Guilds.Count; i++)
            {
                var guild = Context.Client.Guilds.ElementAt(i);
                await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { guild.Id.ToString(CultureInfo.InvariantCulture), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" }).ConfigureAwait(false);
            }

            await message.ModifyAsync((r) => r.Content = "Refreshing prefixes!").ConfigureAwait(true);
            CommandHandler.RefreshPrefix();
            CommandHandler.BotBusy = false;
            await message.ModifyAsync((r) => r.Content = "Database rebuild! Bot now available and listening for all commands!").ConfigureAwait(true);
        }
        [Command("bot rebuild servers")]
        [Remarks("bot rebuild servers")]
        [Summary("Rebuilds the servers part of DB, bot will be unavailable while rebuilding")]
        [Attributes.RequireOwner]
        public async Task RebuildDB_Servers(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                mCache.Set("rebuildDB_ServerAll", random, TimeSpan.FromSeconds(45));
                await this.ReplyAsync($"This is a dangerous operation! All server settings will be lost! Type \"{random}\" to rebuild it! This key will expire in 45 seconds!").ConfigureAwait(false);
                return;
            }
            if (!mCache.TryGetValue("rebuildDB_ServerAll", out string entry))
            { await this.ReplyAsync("Key expired or not created!").ConfigureAwait(false); return; }
            if (entry != key)
            { await this.ReplyAsync("Incorrect key!").ConfigureAwait(false); return; }
            mCache.Remove("rebuildDB_ServerAll");
            var msg = await this.ReplyAsync("Bot will be unavailable for a while. Rebuilding the database.....\nRefactoring server table...").ConfigureAwait(true);
            CommandHandler.BotBusy = true;
            await SqLite.Connection.ExecuteSQL("DELETE FROM servers").ConfigureAwait(false);
            for (var i = 0; i < Context.Client.Guilds.Count; i++)
            {
                var guild = Context.Client.Guilds.ElementAt(i);
                await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { guild.Id.ToString(CultureInfo.InvariantCulture), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" }).ConfigureAwait(false);
            }
            await msg.ModifyAsync((r) => r.Content = "Refreshing prefixes!").ConfigureAwait(true);
            CommandHandler.RefreshPrefix();
            CommandHandler.BotBusy = false;
            await msg.ModifyAsync((r) => r.Content = "Database rebuild! Bot now available and listening for all commands!").ConfigureAwait(true);
        }
        [Command("bot rebuild server")]
        [Remarks("bot rebuild server")]
        [Summary("Rebuilds the DB for only this server")]
        [Attributes.RequireOwner]
        [RequireContext(ContextType.Guild)]
        public async Task RebuildServerDB(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                mCache.Set("rebuildDB_ServerThis", random, TimeSpan.FromSeconds(45));
                await this.ReplyAsync($"This is a dangerous operation! This server settings will be lost! Type \"{random}\" to rebuild it! This key will expire in 45 seconds!").ConfigureAwait(false);
                return;
            }
            if (!mCache.TryGetValue("rebuildDB_ServerThis", out string entry))
            { await this.ReplyAsync("Key expired or not created!").ConfigureAwait(false); return; }
            if (entry != key)
            { await this.ReplyAsync("Incorrect key!").ConfigureAwait(false); return; }
            mCache.Remove("rebuildDB_ServerThis");
            var msg = await this.ReplyAsync("Rebuilding the database....").ConfigureAwait(true);
            await SqLite.Connection.RemoveRecordAsync("servers", $"guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
            await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { Context.Guild.Id.ToString(CultureInfo.InvariantCulture), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" }).ConfigureAwait(false);
            CommandHandler.Prefix.Remove(Context.Guild.Id);
            await msg.ModifyAsync((r) => r.Content = "Done!").ConfigureAwait(true);
        }
        [Command("choose")]
        [Summary("Returns a random item that you supplied (splitting by comma character)")]
        [Remarks("choose <items>")]
        [Alias("random")]
        public Task Choose([Remainder]string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;
            var array = text.Split(',');
            Random rnd = new Random();
            this.ReplyAsync($"I have chosen: {array[rnd.Next(array.Length - 1)]}").GetAwaiter();
            return Task.CompletedTask;
        }
        [Command("emojisay")]
        [Summary("Get's the emoji from a server (nitro is gay)")]
        [Remarks("emojisay <serverid> <emoji name> [--notext] [--s]")]
        public async Task EmojiSay(ulong serverid, [Remainder]string emojiname)
        {
            if (serverid == 0 || string.IsNullOrWhiteSpace(emojiname)) return;
            var noText = false;
            if (emojiname.Contains(" --notext", StringComparison.Ordinal))
            {
                emojiname = emojiname.Replace(" --notext", "", StringComparison.Ordinal);
                noText = true;
            }
            if (emojiname.Contains(" --s", StringComparison.Ordinal))
            {
                emojiname = emojiname.Replace(" --s", "", StringComparison.Ordinal);
                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                    await Context.Message.DeleteAsync().ConfigureAwait(false);
            }
            var guild = Context.Client.GetGuild(serverid);
            if (guild == null)
            {
                await this.ReplyAsync("❌I'm not in that server!").ConfigureAwait(false);
                return;
            }
            var emoteArray = guild.Emotes.ToArray();
            var emojiString = string.Empty;
            foreach (var t in emoteArray)
            {
                if (t.Name != emojiname) continue;
                emojiString = $"<{(t.Animated ? "a" : "")}:{t.Name}:{t.Id}>";
                break;
            }

            if (string.IsNullOrEmpty(emojiString))
                await this.ReplyAsync("❌Emoji not found on that server!").ConfigureAwait(false);
            else
                await this.ReplyAsync($"{(noText ? "" : "Here's your emoji: ")}{emojiString}").ConfigureAwait(false);
        }
    }
}
