using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : RequireContextAttribute
    {
        private readonly ulong _requiredRole;

        public RequireRoleAttribute(ulong requiredRole) : base(ContextType.Guild) => this._requiredRole = requiredRole;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (baseResult.IsSuccess && ((IGuildUser)context.User).RoleIds.Contains(this._requiredRole))
                return PreconditionResult.FromSuccess();
            return baseResult;
        }
    }
}
