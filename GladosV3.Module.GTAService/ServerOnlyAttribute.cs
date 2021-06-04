using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GladosV3.Module.GTAService
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ServerOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) => context.Guild == null ? Task.FromResult(PreconditionResult.FromError("hidden")) : Task.FromResult(context.Guild.Id != 783348387105865778 ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess());
    }
}
