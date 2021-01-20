using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.GTAService
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ServerOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null) { return Task.FromResult(PreconditionResult.FromError("hidden")); }
            if(context.Guild.Id != 783348387105865778) { return Task.FromResult(PreconditionResult.FromError("hidden")); }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
