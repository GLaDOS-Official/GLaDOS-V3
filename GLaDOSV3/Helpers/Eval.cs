using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GladosV3.Helpers
{
   public class Eval
    {
        public class Globals
        {
            public Discord.WebSocket.SocketUserMessage Message => this.Context.Message;
            public Discord.WebSocket.ISocketMessageChannel Channel => this.Context.Message.Channel;
            public Discord.WebSocket.SocketGuild Guild => this.Context.Guild;
            public Discord.WebSocket.SocketUser User => this.Context.Message.Author;
            public Newtonsoft.Json.Linq.JObject Config => Tools.GetConfigAsync(1).GetAwaiter().GetResult();
            public Discord.WebSocket.DiscordSocketClient Client => this.Context.Client;
            public Helpers.Tools Tools => new Tools();
            public Discord.Commands.SocketCommandContext Context { get; private set; }

            public Globals(SocketCommandContext ctx)
            {
                this.Context = ctx;
            }
        }
        public static async Task<string> EvalTask(SocketCommandContext ctx, string cScode)
        {
            List<string> imports = new List<string>(16)
            {
                "System", "System.Collections.Generic", "System.Reflection", "System.Text", "System.Threading.Tasks","System.Linq","System.Math",
                "System.IO","Microsoft.Extensions.Configuration","System.Diagnostics","GladosV3.Helpers","Discord","Discord.Commands","Discord.WebSocket","Newtonsoft.Json"
            };
            try
            {
                ScriptOptions options = ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location))).WithImports(imports).WithEmitDebugInformation(true);
                Script result = CSharpScript.Create(cScode, options, typeof(Globals));
                var returnVal = result.RunAsync(new Globals(ctx)).GetAwaiter().GetResult().ReturnValue?.ToString();
                if (!string.IsNullOrWhiteSpace(returnVal) && returnVal?.ToString().Contains(Tools.GetConfigAsync(0).GetAwaiter().GetResult()["tokens:discord"]))
                   returnVal = returnVal?.Replace(Tools.GetConfigAsync(0).GetAwaiter().GetResult()["tokens:discord"].ToString(),
                        "Nah, no token leak 4 u.");
                return !string.IsNullOrWhiteSpace(returnVal) ? await Task.FromResult( $"**Executed!**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, returnVal)}```").ConfigureAwait(true) : await Task.FromResult("**Executed!** *No output.*");
            }
            catch (CompilationErrorException e)
            {
                return await Task.FromResult<string>($"**Compiler error**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, e.Diagnostics)}```");
            }
            catch (Exception e)
            {
                return await Task.FromResult<string>($"**Exception!**{e.Message}\n{e.StackTrace}");
            }
        }

    }
}
