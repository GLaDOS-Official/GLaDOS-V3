using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace GladosV3.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class NsfwAttribute : PreconditionAttribute // what am I doing with my life https://i.gyazo.com/b7ec8262325554b9c8a9e724521ee532.png
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(context.Channel.IsNsfw ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("This command must be used in NSFW channel!"));
        }
    }
}
