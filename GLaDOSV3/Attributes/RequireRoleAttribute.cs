using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireRoleAttribute : RequireContextAttribute
    {
        private readonly ulong _requiredRole;

        public RequireRoleAttribute(ulong requiredRole) : base(ContextType.Guild)
        {
            _requiredRole = requiredRole;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissions(context, command, services);
            if (baseResult.IsSuccess && ((Discord.IGuildUser)context.User).RoleIds.Contains(_requiredRole))
                return PreconditionResult.FromSuccess();
            else
                return baseResult;
        }
    }
}
