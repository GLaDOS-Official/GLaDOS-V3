using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Modules
{

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

        [Command("sudo")]
        [Summary("sudo <command>")]
        [Remarks("Execute bot command as if you're guild owner")]
        [Discord.Commands.RequireOwner]
        public async Task Sudo(string command)
        {
            object clone = Clone(Context.Message);
            var result = _service.Search(Context, command);
            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }
            FieldInfo a =  typeof(SocketUserMessage).GetField("Content", BindingFlags.Instance | BindingFlags.NonPublic);
            a.SetValue(clone,"Hello");
            var context = new SocketCommandContext(Context.Client,Context.Message);
            await _service.ExecuteAsync(context, 1, _provider);
        }
        [Command("eval")]
        [Summary("eval <c# code>")]
        [Remarks("Execute c# code")]
        [Discord.Commands.RequireOwner]
        public async Task eval([Remainder]string CsharpCode)
        {
            await ReplyAsync(Eval.evalTask(CsharpCode, false).GetAwaiter().GetResult());
        }
        public static T Clone<T>(T obj)
        {
            var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (T)inst?.Invoke(obj, null);
        }
    }
}
