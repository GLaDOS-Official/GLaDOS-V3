using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    public sealed class Eval
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public class Globals
        {
            public SocketUserMessage Message => Context.Message;
            public ISocketMessageChannel Channel => Context.Message.Channel;
            public SocketGuild Guild => Context.Guild;
            public SocketUser User => Context.Message.Author;
            public BotSettingsHelper<string> Config => new BotSettingsHelper<string>();

            public DiscordSocketClient Client => Context.Client;
            public Tools Tools = new Tools();
            public SocketCommandContext Context { get; }

            public SQLiteConnection SQLConnection = SqLite.Connection;

            public Globals(SocketCommandContext ctx) => Context = ctx;
        }
        public static async Task<string> EvalTask(SocketCommandContext ctx, string cScode)
        {
            List<string> imports = new List<string>(16)
            {
                "System", "System.Collections.Generic", "System.Reflection", "System.Text", "System.Threading.Tasks","System.Linq","System.Math",
                "System.IO","System.Diagnostics","GLaDOSV3.Helpers","Discord","Discord.Commands","Discord.WebSocket","Newtonsoft.Json",
                "System.Data.SQLite", "Microsoft.Extensions"
            };
            try
            {
                ScriptOptions options = ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location))).WithImports(imports).WithEmitDebugInformation(true);
                Script result = CSharpScript.Create(cScode, options, typeof(Globals));
                var returnVal = result.RunAsync(new Globals(ctx)).GetAwaiter().GetResult().ReturnValue?.ToString();
                BotSettingsHelper<string> r = new BotSettingsHelper<string>();
                var token = r["tokens_discord"];
                if (!string.IsNullOrWhiteSpace(returnVal) && returnVal.Contains(token, StringComparison.Ordinal)) returnVal = returnVal?.Replace(token, "Nah, no token leak 4 u.", StringComparison.OrdinalIgnoreCase);

                return !string.IsNullOrWhiteSpace(returnVal) ? await Task.FromResult($"**Executed!**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, returnVal)}```").ConfigureAwait(true) : await Task.FromResult("**Executed!** *No output.*").ConfigureAwait(false);
            }
            catch (CompilationErrorException e)
            {
                return $"**Compiler error**\nOutput: ```{string.Join('\n', e.Diagnostics)}```";
            }
            catch (Exception e)
            {
                return $"**Exception!** {e.Message}\n{e.StackTrace}";
            }
        }
    }
}
