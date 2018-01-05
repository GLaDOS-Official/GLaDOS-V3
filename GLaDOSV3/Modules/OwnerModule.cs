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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GladosV3.Modules
{
    //[Name("Bot owner")]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        public OwnerModule(CommandService service, IConfigurationRoot config, IServiceProvider provider)
        {
            _service = service;
            _config = config;
            _provider = provider;
        }

        
        [Group("Bot")]
        [Attributes.RequireOwner]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("maintenance")]
            [Summary("bot maintenance")]
            [Remarks("Toggles maintenance mode on or off")]

            public async Task Maintenance()
            {
                JObject clasO = Tools.GetConfig(1).GetAwaiter().GetResult();
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
            [Summary("bot restart")]
            [Remarks("Restarts the bot")]
            public async Task Restart()
            {
                await ReplyAsync($"Restarting the bot!");
                Tools.RestartApp();
            }
            [Command("shutdown")]
            [Summary("bot shutdown")]
            [Remarks("Shutsdown the bot")]
            public async Task Shutdown()
            {
                await ReplyAsync($"Shutting down the bot! :wave:");
                Environment.Exit(0);
            }
            [Command("username")]
            [Summary("bot username <username>")]
            [Remarks("Sets bot's username")]
            public async Task Username([Remainder]string username)
            {
                JObject clasO =
                    Tools.GetConfig(1).GetAwaiter().GetResult();
                clasO["name"] = username;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                await ReplyAsync($"Set bot's username to {clasO["name"].Value<string>()}.\nRestarting the bot!");
                Tools.RestartApp();
            }
            [Command("eval")]
            [Summary("bot eval <code>")]
            [Remarks("Execute c# code")]
            [Attributes.RequireOwner]
            public async Task Eval([Remainder]string code)
            {
                var message = await ReplyAsync("Please wait...");
                await message.ModifyAsync(properties => properties.Content = Helpers.Eval.EvalTask(Context, code).GetAwaiter().GetResult());
            }
            [Command("game")]
            [Summary("bot game [game]")]
            [Remarks("Set's bot game state")]
            [Attributes.RequireOwner]
            public async Task Game([Remainder]string status = "")
            {
                JObject clasO =
                    Tools.GetConfig(1).GetAwaiter().GetResult();
                if(status == null)
                    await Context.Client.SetGameAsync(null);
                clasO["discord"]["game"] = status;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                if(status == "")
                    await ReplyAsync($"Reseted bot's game state\nRestarting the bot!");
                else
                    await ReplyAsync($"Set bot's game state to {clasO["discord"]["game"].Value<string>()}.\nRestarting the bot!");
                Tools.RestartApp();
            }
            [Command("status")]
            [Summary("bot status <status>")]
            [Remarks("Set's bot status")]
            [Attributes.RequireOwner]
            public async Task Status([Remainder]string status = "")
            {
                JObject clasO =
                    Tools.GetConfig(1).GetAwaiter().GetResult();
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
