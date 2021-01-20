using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context == null) return PreconditionResult.FromError("oof");
            return context.Client.TokenType switch
            {
                TokenType.Bot => await IsOwner.CheckPermission(context).ConfigureAwait(false) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Command can only be run by the owner of the bot"),
                _ => PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}."),
            };
        }
    }
}
