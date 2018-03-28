using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;

namespace GladosV3.Helpers
{
   public class Eval
   {
        public class Globals
        {
            public SocketUserMessage Message => Context.Message;
            public ISocketMessageChannel Channel => Context.Message.Channel;
            public SocketGuild Guild => Context.Guild;
            public SocketUser User => Context.Message.Author;
            public JObject Config => Tools.GetConfigAsync(1).GetAwaiter().GetResult();
            public DiscordSocketClient Client => Context.Client;
            public Tools Tools => new Tools();
            public SocketCommandContext Context { get; private set; }

            public Globals(SocketCommandContext ctx)
            {
                Context = ctx;
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
                if (!string.IsNullOrWhiteSpace(returnVal) && returnVal?.Contains(Tools.GetConfigAsync(0).GetAwaiter().GetResult()["tokens:discord"]))
                   returnVal = returnVal?.Replace(Tools.GetConfigAsync(0).GetAwaiter().GetResult()["tokens:discord"].ToString(),
                        "Nah, no token leak 4 u.");
                return !string.IsNullOrWhiteSpace(returnVal) ? await Task.FromResult( $"**Executed!**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, returnVal)}```").ConfigureAwait(true) : await Task.FromResult("**Executed!** *No output.*");
            }
            catch (CompilationErrorException e)
            {
                return await Task.FromResult($"**Compiler error**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, e.Diagnostics)}```");
            }
            catch (Exception e)
            {
                return await Task.FromResult($"**Exception!**{e.Message}\n{e.StackTrace}");
            }
        }
   }
}
