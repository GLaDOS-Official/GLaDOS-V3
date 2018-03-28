using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Helpers;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = await Tools.GetConfigAsync();
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync();
                    return (context.User.Id != application.Owner.Id && ulong.Parse(config["ownerID"]) != context.User.Id && !IsOwner.IsCoOwner(config, context.User.Id)) ? PreconditionResult.FromError("Command can only be run by the owner of the bot") : PreconditionResult.FromSuccess(); //&& (IConfigurationRoot)config["co-owners"].ToString().Split(',').Any(t => t == context.User.Id))
                case TokenType.User:
                    return (context.User.Id != context.Client.CurrentUser.Id) ? PreconditionResult.FromError("Command can only be run by the owner of the bot") : PreconditionResult.FromSuccess();
                default:
                    return PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }

    public class IsOwner
    {
        public static bool IsCoOwner(IConfigurationRoot _config, ulong ID)
        {
            bool fail = true;
            for (int i = 0; i < 30; i++)
            {
                if (_config[$"co-owners:{i}"] == null)
                    break;
                if (ID == ulong.Parse(_config[$"co-owners:{i}"]))
                { fail = false; break; }
            }
            return !fail;
        }
        public static async Task<bool> CheckPermission(ICommandContext context)
        {
            var config = await Tools.GetConfigAsync();
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                        return (ulong.Parse(config["ownerID"]) != context.User.Id && context.User.Id != context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id && !IsCoOwner(config, context.User.Id));
                case TokenType.User:
                        return (context.User.Id != context.Client.CurrentUser.Id);
                default:
                    return false;
            }
        }
        public static async Task<ulong> GetOwner(ICommandContext context)
        {
            var config = await Tools.GetConfigAsync();
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                        return context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id;
                case TokenType.User:
                        return ulong.Parse(config["ownerID"]);
                default:
                    return UInt64.MaxValue;
            }
        }
    }
}
