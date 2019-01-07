using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireMFAAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.MfaLevel == MfaLevel.Enabled ? Task.FromResult(context.Client.CurrentUser.IsMfaEnabled != true ? PreconditionResult.FromError("The owner of this bot has MFA disabled!") : PreconditionResult.FromSuccess()) : Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
