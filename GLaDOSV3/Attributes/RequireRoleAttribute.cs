using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : RequireContextAttribute
    {
        private readonly ulong _requiredRole;

        public RequireRoleAttribute(ulong requiredRole) : base(ContextType.Guild) => this._requiredRole = requiredRole;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context == null) return PreconditionResult.FromError("oof");
            var baseResult = await base.CheckPermissionsAsync(context, command, services).ConfigureAwait(true);
            return baseResult.IsSuccess && ((IGuildUser)context.User).RoleIds.Contains(this._requiredRole)
                ? PreconditionResult.FromSuccess()
                : baseResult;
        }
    }
}
