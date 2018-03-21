using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.NomadCI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class NomadOnly : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = context.Client.GetApplicationInfoAsync();
                    return Task.FromResult(context.Guild.Id != 259776446942150656 ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess());
                default:
                    return Task.FromResult(PreconditionResult.FromError("hidden"));
            }
        }
    }
}
