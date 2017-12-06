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
        [Helpers.RequireOwner]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("maintenance")]
            [Summary("bot maintenance")]
            [Remarks("Toggles maintenance mode on or off")]

            public async Task Maintenance()
            {
                JObject clasO =
                    JObject.Parse(File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json")).GetAwaiter().GetResult());
                if (clasO["maintenance"].Value<bool>() == false)
                    clasO["maintenance"] = true;
                else
                    clasO["maintenance"] = false;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"),clasO.ToString());
                await ReplyAsync($"Set maintenance mode to {clasO["maintenance"].Value<string>().ToLower()}\nRestarting the bot!");
                RestartApp.Restart();
            }
            [Command("restart")]
            [Summary("bot restart")]
            [Remarks("Restarts the bot")]
            public async Task Restart()
            {
                await ReplyAsync($"Restarting the bot!");
                RestartApp.Restart();
            }
            [Command("username")]
            [Summary("bot username")]
            [Remarks("Sets bot's username")]
            public async Task Username([Remainder]string username)
            {
                JObject clasO =
                    JObject.Parse(File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json")).GetAwaiter().GetResult());
                clasO["name"] = username;
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), clasO.ToString());
                await ReplyAsync($"Set bot's username to {clasO["name"].Value<string>()}\nRestarting the bot!");
                RestartApp.Restart();
            }
            [Command("eval")]
            [Summary("bot eval <code>")]
            [Remarks("Execute c# code")]
            [Helpers.RequireOwner]
            public async Task Eval([Remainder]string code)
            {
                await ReplyAsync(Helpers.Eval.EvalTask(code).GetAwaiter().GetResult());
            }
        }
    }
}
