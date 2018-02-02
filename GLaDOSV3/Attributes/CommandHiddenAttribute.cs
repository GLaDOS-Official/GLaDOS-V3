using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Helpers;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandHiddenAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = context.Client.GetApplicationInfoAsync();
                    return Task.FromResult(context.User.Id != application.Result.Owner.Id ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess());
                case TokenType.User:
                    return Task.FromResult(context.User.Id != context.Client.CurrentUser.Id ? PreconditionResult.FromError("hidden") : PreconditionResult.FromSuccess());
                default: return Task.FromResult(PreconditionResult.FromError("hidden"));
            }
        }
    }
}
