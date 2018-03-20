using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace GladosV3.Modules
{
    //[Name("Bot owner")]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        public readonly CommandService _service;
        public readonly IConfigurationRoot _config;
        public readonly IServiceProvider _provider;

        // CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider;
        public OwnerModule(CommandService service, IConfigurationRoot config, IServiceProvider provider)
        {
            _service = service;
            _config = config;
            _provider = provider;
        }


        [Group("Bot")]
        [Attributes.CommandHidden]
        [Attributes.RequireOwner]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("maintenance")]
            [Remarks("bot maintenance")]
            [Summary("Toggles maintenance mode on or off")]

            public async Task Maintenance()
            {
                JObject clasO = Tools.GetConfigAsync(1).GetAwaiter().GetResult();
                if (clasO["maintenance"].Value<bool>() == false)
                    clasO["maintenance"] = true;
                else
                    clasO["maintenance"] = false;
                string status = clasO["maintenance"].Value<bool>() ? "enabled" : "disabled";
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"),clasO.ToString());
                await ReplyAsync($"Set maintenance mode to: {status}.\nRestarting the bot!");
                Tools.RestartApp();
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
                await ReplyAsync($"Shutting down the bot! :wave:");
                Environment.Exit(0);
            }
            [Command("username")]
            [Remarks("bot username <username>")]
            [Summary("Sets bot's username")]
            public async Task Username([Remainder]string username)
            {
                JObject clasO =
                    Tools.GetConfigAsync(1).GetAwaiter().GetResult();
                clasO["name"] = username;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                await ReplyAsync($"Set bot's username to {clasO["name"].Value<string>()}.\nRestarting the bot!");
                Tools.RestartApp();
            }
            [Command("eval")]
            [Remarks("bot eval <code>")]
            [Summary("Execute c# code")]
            [Attributes.RequireOwner]
            public async Task Eval([Remainder]string code)
            {
                var message = await ReplyAsync("Please wait...");
                await message.ModifyAsync(properties => properties.Content = Helpers.Eval.EvalTask(Context, code).GetAwaiter().GetResult());
            }
            [Command("message")]
            [Remarks("bot message <system message>")]
            [Summary("Sends message to all servers!")]
            [Attributes.RequireOwner]
            public async Task Message([Remainder]string message)
            {
                var progress = await ReplyAsync("Sending...");
                foreach (var t in Context.Client.Guilds) {
                    if(t.DefaultChannel != null)
                        await t.DefaultChannel.SendMessageAsync($"System message: {message}");
                    else
                        await t.TextChannels.ToArray()[0].SendMessageAsync($"System message: {message}");
                }
                var correctSpellingEnglishIHateIt = Context.Client.Guilds.Count <= 1 ? "guild" : "guilds";
                await progress.ModifyAsync(properties => properties.Content = $"Done! Sent to {Context.Client.Guilds.Count} {correctSpellingEnglishIHateIt}.");
            }
            [Command("game")]
            [Remarks("bot game [game]")]
            [Summary("Set's bot game state")]
            [Attributes.RequireOwner]
            public async Task Game([Remainder]string status = "")
            {
                JObject clasO =
                    Tools.GetConfigAsync(1).GetAwaiter().GetResult();
                if(status == null)
                    await Context.Client.SetGameAsync(null);
                clasO["discord"]["game"] = status;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                if(status == "")
                    await ReplyAsync($"Reset bot's game state\nRestarting the bot!");
                else
                    await ReplyAsync($"Set bot's game state to {clasO["discord"]["game"].Value<string>()}.\nRestarting the bot!");
                Tools.RestartApp();
            }

            [Command("blacklist")]
            [Remarks("bot blacklist <add/remove> <userid> [Reason]")]
            [Summary("Blacklists a user from using the bot")]
            [Attributes.RequireOwner]
            public async Task Blacklist(string method, ulong userid, [Remainder] string reason = "Unspecified")
            {
                switch (method)
                {
                    case "add":
                        await SqLite.Connection.AddRecordAsync("BlacklistedUsers","UserId,Reason,Date",new []{userid.ToString(),reason,DateTime.Now.ToString()}).ConfigureAwait(false);
                        break;
                    case "remove":
                        await SqLite.Connection.RemoveRecordAsync("BlacklistedUsers",$"UserID={userid.ToString()}").ConfigureAwait(false);
                        break;
                    default:
                        await ReplyAsync("Invalid method.");
                        break;  
                }
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
                JObject clasO =
                    Tools.GetConfigAsync(1).GetAwaiter().GetResult();
                if (status != "online" && status != "invisible" && status != "afk" && status != "donotdisturb")
                { await ReplyAsync("Valid statuses are: online, invisible, afk, donotdisturb"); return;}
                clasO["discord"]["status"] = status;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"),
                    clasO.ToString());
                await ReplyAsync(
                        $"Set bot's game state to {clasO["discord"]["status"].Value<string>()}.\nRestarting the bot!");
                Tools.RestartApp();
            }
        }
    }
}
