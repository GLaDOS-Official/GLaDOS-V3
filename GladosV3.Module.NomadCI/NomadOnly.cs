using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace GladosV3.Module.NomadCI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class NomadOnly : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = context.Client.GetApplicationInfoAsync();
                    return Task.FromResult(context.Guild.Id != BuilderService.Config["nomad"]["serverID"].Value<ulong>() ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess());
                default:
                    return Task.FromResult(PreconditionResult.FromError("hidden"));
            }
        }
    }
}
