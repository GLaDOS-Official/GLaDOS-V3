using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Attributes;
using GLaDOSV3.Helpers;
using GLaDOSV3.Services;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GLaDOSV3.Modules
{
    //[Name("Bot owner")]
    public class OwnerModule : ModuleBase<ShardedCommandContext>
    {
        private readonly CommandService service;
        private readonly IServiceProvider provider;
        private readonly BotSettingsHelper<string> botSettingsHelper;


        // CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider;
        public OwnerModule(CommandService service, IServiceProvider provider, BotSettingsHelper<string> botSettingsHelper)
        {
            this.service = service;
            this.provider = provider;
            this.botSettingsHelper = botSettingsHelper;
        }

        [Command("bot maintenance")]
        [Remarks("bot maintenance [reason]")]
        [Summary("Toggles maintenance mode on or off")]
        [Attributes.RequireOwner]
        public async Task Maintenance([Remainder] string reason = "")
        {
            CommandHandler.MaintenanceMode = reason;
            IsOwner.BotSettingsHelper["maintenance"] = reason;
            await this.ReplyAsync($"{(string.IsNullOrWhiteSpace(reason) ? "Disabled" : "Enabled")} maintenance reason{(string.IsNullOrWhiteSpace(reason) ? "" : " to: ")}{(string.IsNullOrWhiteSpace(reason) ? "" : reason)}!").ConfigureAwait(false);
        }
        [Command("bot restart")]
        [Remarks("bot restart")]
        [Summary("Restarts the bot")]
        [Attributes.RequireOwner]
        public async Task Restart()
        {
            await this.ReplyAsync("Restarting the bot!").ConfigureAwait(false);
            Tools.RestartApp();
        }
        [Command("bot shutdown")]
        [Remarks("bot shutdown")]
        [Summary("Shutdowns the bot")]
        [Attributes.RequireOwner]
        public async Task Shutdown()
        {
            await this.ReplyAsync("Shutting down the bot! 👋").ConfigureAwait(true);
            Environment.Exit(0);
        }

        [Command("bot username")]
        [Remarks("bot username <username>")]
        [Summary("Sets bot's username")]
        [Attributes.RequireOwner]
        public async Task Username([Remainder] string username)
        {
            IsOwner.BotSettingsHelper["name"] = username;
            await this.ReplyAsync($"Set bot's username to {username}.").ConfigureAwait(false);
            await Context.Client.CurrentUser.ModifyAsync(properties => properties.Username = username).ConfigureAwait(false);
        }
        [Command("bot eval")]
        [Remarks("bot eval <code>")]
        [Summary("Execute c# code")]
        [Attributes.RequireOwner]
        public async Task Eval([Remainder] string code)
        {
            IUserMessage message = await this.ReplyAsync("Please wait...").ConfigureAwait(false);
            await message.ModifyAsync(async properties => properties.Content = await Helpers.Eval.EvalTask(Context, code).ConfigureAwait(true)).ConfigureAwait(false);
        }

        [Command("bot sudo")]
        [Remarks("bot sudo <user> <command>")]
        [Summary("Execute bot command as another user")]
        [Attributes.RequireOwner]
        public async Task Sudo(SocketUser user, [Remainder] string command)
        {
            ConstructorInfo ctor = typeof(SocketUserMessage).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            //var state = Context.Client.GetType().GetMethod("get_State",
            //                                               BindingFlags.Instance
            //                                               | BindingFlags.NonPublic)
            //                  ?.Invoke(Context.Client, null);
            var msg = ctor.Invoke(
                          new object[]
                          {
                              Context.Client, Context.Message.Id,
                              Context.Channel, user, user.IsBot ? MessageSource.Bot : MessageSource.User
                          });
            await this.service.ExecuteAsync(new ShardedCommandContext(Context.Client, (SocketUserMessage)msg), command.StartsWith(this.botSettingsHelper["prefix"]) ? command[this.botSettingsHelper["prefix"].Length..] : command, this.provider);
        }
        //[Command("bot webhookmass")]
        //[Remarks("bot webhookmass <serverid> <count>")]
        //[Summary("Add webhook to every channel")]
        //[Attributes.RequireOwner]
        //public async Task WebHookMass(ulong serverId, int number = 1)
        //{
        //    SocketGuild guild = Context.Client.GetGuild(serverId);
        //    string result = "";
        //    foreach (SocketTextChannel sc in guild.TextChannels)
        //    {
        //        for (var i = 0; i < number; i++)
        //        {
        //            var hook = await sc.CreateWebhookAsync("Captain hook").ConfigureAwait(true);
        //            var id = hook.Id;
        //            var token = hook.Token;
        //            result += $"https://canary.discordapp.com/api/webhooks/{id}/{token}\n";
        //        }
        //    }
        //    var dm = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
        //    foreach (var msg in Tools.SplitMessage(result, 1985))
        //        await dm.SendMessageAsync($"```\n{msg}```").ConfigureAwait(false);
        //    await dm.CloseAsync().ConfigureAwait(false);
        //}
        //[Command("bot rehook")]
        //[Remarks("bot rehook <user> [--s]")]
        //[Summary("Hooks his permissions to admin to every channel")]
        //[Attributes.RequireOwner]
        //public async Task ReHook(SocketUser user, [Remainder] string silent = "")
        //{
        //    var silentB = false;
        //    IUserMessage message = null;
        //    if (silent == "--s")
        //        silentB = true;

        //    if (silentB)
        //        await Context.Message.DeleteAsync().ConfigureAwait(false);
        //    else
        //        message = await this.ReplyAsync("Hooking....").ConfigureAwait(false);

        //    IReadOnlyCollection<SocketGuildChannel> channels = Context.Guild.Channels;
        //    for (var i = 0; i < channels.Count; i++)
        //    {
        //        SocketGuildChannel channel = channels.ElementAt(i);
        //        if (channel.GetPermissionOverwrite(user) == null)
        //            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow)).ConfigureAwait(false);
        //        else
        //            channel.GetPermissionOverwrite(user)?.Modify(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);
        //    }
        //    if (silentB)
        //        return;
        //    await message.ModifyAsync(a => a.Content = "Done!").ConfigureAwait(false);
        //}
        [Command("bot message")]
        [Remarks("bot message <system message>")]
        [Summary("Sends message to all servers!")]
        [Attributes.RequireOwner]
        public async Task Message([Remainder] string message)
        {
            IUserMessage progress = await this.ReplyAsync("Sending...").ConfigureAwait(false);
            foreach (SocketGuild t in Context.Client.Guilds)
            {
                if (t.DefaultWritableChannel() != null)
                {
                    await t.DefaultWritableChannel().SendMessageAsync($"System message: {message}").ConfigureAwait(false);
                }
                else
                {
                    await t.TextChannels.ToArray()[0].SendMessageAsync($"System message: {message}").ConfigureAwait(false);
                }
            }
            await progress.ModifyAsync(properties => properties.Content = $"Done! Sent to {Context.Client.Guilds.Count} {(Context.Client.Guilds.Count <= 1 ? "guild" : "guilds")}.").ConfigureAwait(false);
        }
        [Command("bot game")]
        [Remarks("bot game [game]")]
        [Summary("Set's bot game state")]
        [Attributes.RequireOwner]
        public async Task Game([Remainder] string status = "")
        {
            if (status == null)
            {
                await Context.Client.SetGameAsync(null).ConfigureAwait(false);
            }
            IsOwner.BotSettingsHelper["discord_game"] = status;
            if (string.IsNullOrEmpty(status))
            {
                await this.ReplyAsync("Reset bot's game state.").ConfigureAwait(false);
            }
            else
            {
                await this.ReplyAsync($"Set bot's game state to {status}.").ConfigureAwait(false);
            }

            await Context.Client.SetGameAsync(status).ConfigureAwait(false);
        }
        [Command("bot blacklist user add")]
        [Remarks("bot blacklist user add <userid> [Reason]")]
        [Summary("Blacklists a user from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistUserAdd(ulong userid, [Remainder] string reason = "Unspecified")
        {
            await SqLite.Connection.AddRecordAsync("BlacklistedUsers", "UserId,Date,Reason", new[] { userid.ToString(CultureInfo.InvariantCulture), DateTime.Now.ToString(CultureInfo.InvariantCulture), reason }).ConfigureAwait(true);
            CommandHandler.BlacklistedUsers.Add(userid);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot blacklist users")]
        [Remarks("bot blacklist users")]
        [Summary("Blacklists a user from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistUsers()
        {
            using DataTable dt = await SqLite.Connection.GetValuesAsync("BlacklistedUsers").ConfigureAwait(true);
            if (dt.Rows.Count <= 0)
            {
                await this.ReplyAsync("No users are blocked.").ConfigureAwait(false);
                return;
            }
            var output = "User (Mention), Date, Reason\n";
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                output +=
                    $"{dt.Rows[i]["UserId"]} (<@{dt.Rows[i]["UserId"]}>), {dt.Rows[i]["Date"]}, {dt.Rows[i]["Reason"]}\n";
            }
            await this.ReplyAsync(output).ConfigureAwait(false);
        }
        [Command("bot blacklist user remove")]
        [Remarks("bot blacklist user remove <userid>")]
        [Summary("Blacklists a user from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistUserRemove(ulong userid)
        {
            await SqLite.Connection.RemoveRecordAsync("BlacklistedUsers", $"UserID={userid.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(true);
            CommandHandler.BlacklistedUsers.Remove(userid);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot blacklist guild add")]
        [Remarks("bot blacklist guild add <guildid> [Reason]")]
        [Summary("Blacklists a guild from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistServerAdd(ulong guildid, [Remainder] string reason = "Unspecified")
        {
            await SqLite.Connection.AddRecordAsync("BlacklistedServers", "guildid,date,reason", new[] { guildid.ToString(CultureInfo.InvariantCulture), DateTime.Now.ToString(CultureInfo.InvariantCulture), reason }).ConfigureAwait(true);
            CommandHandler.BlacklistedServers.Add(guildid);
            for (var i = 0; i < Context.Client.Guilds.Count; i++)
            {
                var guild = Context.Client.Guilds.ElementAt(i);
                if (guild.Id != guildid) continue;
                try
                {
                    await guild.DefaultWritableChannel().SendMessageAsync(
                                                                $"Hello! This server has been blacklisted from using {Context.Client.CurrentUser.Mention}! I will now leave. Have fun without me!").ConfigureAwait(false);
                }
                catch {/* ignored*/}

                await guild.LeaveAsync().ConfigureAwait(false);
                break;
            }
            await SqLite.Connection.RemoveRecordAsync("servers", $"guildid={guildid.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot blacklist guilds")]
        [Remarks("bot blacklist guilds")]
        [Summary("Blacklists a guild from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistServers()
        {
            using DataTable dt = await SqLite.Connection.GetValuesAsync("BlacklistedServers").ConfigureAwait(true);
            if (dt.Rows.Count <= 0)
            {
                await this.ReplyAsync("No servers are blocked.").ConfigureAwait(false);
                return;
            }
            var output = "Guild ID, Date, Reason\n";
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                output +=
                    $"{dt.Rows[i]["guildid"]}, {dt.Rows[i]["date"]}, {dt.Rows[i]["reason"]}\n";
            }
            await this.ReplyAsync(output).ConfigureAwait(false);
        }
        [Command("bot blacklist guild remove")]
        [Remarks("bot blacklist guild remove <guildid>")]
        [Summary("Blacklists a user from using the bot")]
        [Attributes.RequireOwner]
        public async Task BlacklistServerRemove(ulong guildid)
        {
            await SqLite.Connection.RemoveRecordAsync("BlacklistedServers", $"guildid={guildid.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(true);
            CommandHandler.BlacklistedServers.Remove(guildid);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot coowner add")]
        [Remarks("bot coowner add <id>")]
        [Summary("Adds a bot's co-owner")]
        [Attributes.RequireOwner]
        public async Task AddCoOwner(ulong userId)
        {
            var ok = this.botSettingsHelper["co-owners"];
            var coOwners = ok.Split(',').ToList();
            var fail = coOwners.All(t => t != userId.ToString(CultureInfo.InvariantCulture));
            if (!fail) { await this.ReplyAsync("User is already a co-owner!"); return; }
            coOwners.Add(userId.ToString());
            this.botSettingsHelper["co-owners"] = string.Join(",", coOwners);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot coowner remove")]
        [Remarks("bot coowner remove <id>")]
        [Summary("Removes a bot's co-owner")]
        [Attributes.RequireOwner]
        public async Task RemoveCoOwner(ulong userId)
        {
            var ok = this.botSettingsHelper["co-owners"];
            var coOwners = ok.Split(',').ToList();
            var fail = coOwners.All(t => t != userId.ToString(CultureInfo.InvariantCulture));
            if (fail) { await this.ReplyAsync("User isn't already co-owner!"); return; }
            coOwners.Remove(userId.ToString());
            this.botSettingsHelper["co-owners"] = string.Join(",", coOwners);
            await this.ReplyAsync("Ok!").ConfigureAwait(false);
        }
        [Command("bot coowner list")]
        [Remarks("bot coowner list")]
        [Summary("Adds a bot's co-owner")]
        [Attributes.RequireOwner]
        public async Task ListCoOwners()
        {
            var ok = this.botSettingsHelper["co-owners"];
            if (string.IsNullOrWhiteSpace(ok)) { await this.ReplyAsync("This bot has no coowners!"); return; }
            var coOwners = ok.Split(',');
            var output = coOwners.Aggregate("User (Mention)\n", (current, t) => current + $"{t} (<@{t}>)\n");
            await this.ReplyAsync(output).ConfigureAwait(false);
        }
        [Attributes.RequireOwner]
        [Command("bot release")]
        [Remarks("bot release")]
        [Summary("Releases stuff from memory that are not needed")]
        [Attributes.RequireOwner]
        public Task Release()
        {
            Tools.ReleaseMemory();
            this.ReplyAsync("Ok!").GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
        [Command("bot status")]
        [Remarks("bot status <status>")]
        [Summary("Sets bot status")]
        [Attributes.RequireOwner]
        public async Task Status([Remainder] string status = "")
        {

            //JObject clasO = Tools.GetConfigAsync(1).GetAwaiter().GetResult();
            if (status != "Online" && status != "Invisible" && status != "AFK" && status != "DoNotDisturb")
            { await this.ReplyAsync("Valid statuses are: Online, Invisible, AFK, DoNotDisturb").ConfigureAwait(false); return; }
            /*clasO["discord"]["status"] = status;
            await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"),
                clasO.ToString());*/
            IsOwner.BotSettingsHelper["discord_status"] = status;
            await this.ReplyAsync($"Set bot's game state to {status}.").ConfigureAwait(false);
            await Context.Client.SetStatusAsync(Enum.Parse<UserStatus>(status)).ConfigureAwait(false);
        }

        [Attributes.RequireOwner]
        [Command("bot extension load")]
        [Summary("Loads bot extension")]
        [Attributes.RequireOwner]
        public Task LoadExtension([Remainder] string filename)
        {
            if (!ExtensionLoadingService.Load(this.service, filename))
            {
                this.ReplyAsync($"Couldn't find extension with filename: {filename}!");
                return Task.CompletedTask;
            }
            this.ReplyAsync($"Loaded extension with filename: {filename}!");
            return Task.CompletedTask;
        }

        [Attributes.RequireOwner]
        [Command("bot extension reload")]
        [Summary("Reloads bot extension")]
        [Attributes.RequireOwner]
        public Task ReloadExtension([Remainder] string name)
        {
            if (!ExtensionLoadingService.Reload(this.service, name))
            {
                this.ReplyAsync($"Couldn't find extension: {name}!");
                return Task.CompletedTask;
            }
            this.ReplyAsync($"Reloaded extension: {name}!");
            return Task.CompletedTask;
        }

        [Attributes.RequireOwner]
        [Command("bot extension unload")]
        [Summary("Unloads bot extension")]
        [Attributes.RequireOwner]
        public Task UnloadExtension([Remainder] string name)
        {
            if (!ExtensionLoadingService.Unload(this.service, name))
            {
                this.ReplyAsync($"Couldn't find extension: {name}!");
                return Task.CompletedTask;
            }
            this.ReplyAsync($"Unloaded extension: {name}!");
            return Task.CompletedTask;
        }

        [Attributes.RequireOwner]
        [Command("bot extension list")]
        [Summary("List all bot extensions")]
        [Attributes.RequireOwner]
        public Task ListExtensions()
        {
            var msg = ExtensionLoadingService.Extensions.Aggregate("", (current, extension) => current + $"{extension.ModuleName} ({extension.ModulePath})\n");
            this.ReplyAsync($"```\n{msg}\n```");
            return Task.CompletedTask;
        }
    }
}
