using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CommandHiddenAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Client.TokenType switch
            {
                TokenType.Bot => Task.FromResult(IsOwner.CheckPermission(context).GetAwaiter().GetResult() ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess()),
                _ => Task.FromResult(PreconditionResult.FromError("hidden")),
            };
        }
    }
}
