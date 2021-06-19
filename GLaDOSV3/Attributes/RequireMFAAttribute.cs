using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GLaDOSV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequireMfaAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) => context?.Guild.MfaLevel == MfaLevel.Enabled ? Task.FromResult(context.Client.CurrentUser.IsMfaEnabled != true ? PreconditionResult.FromError("The owner of this bot has MFA disabled!") : PreconditionResult.FromSuccess()) : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
