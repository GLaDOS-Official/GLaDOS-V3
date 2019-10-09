using Discord;
using Discord.Commands;
using GladosV3.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync();
                    return (context.User.Id != application.Owner.Id && ulong.Parse(IsOwner.botSettingsHelper["ownerID"]) != context.User.Id && !IsOwner.IsCoOwner(context.User.Id)) ? PreconditionResult.FromError("Command can only be run by the owner of the bot") : PreconditionResult.FromSuccess(); //&& (IConfigurationRoot)config["co-owners"].ToString().Split(',').Any(t => t == context.User.Id))
                /*case TokenType.User:
                    return (context.User.Id != context.Client.CurrentUser.Id) ? PreconditionResult.FromError("Command can only be run by the owner of the bot") : PreconditionResult.FromSuccess();*/
                default:
                    return PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }

    public class IsOwner
    {
        public static BotSettingsHelper<string> botSettingsHelper = new BotSettingsHelper<string>();
        public static bool IsCoOwner(ulong ID)
        {
            string ok = botSettingsHelper["co-owners"];
            if (string.IsNullOrWhiteSpace(ok))
                return false;
            string[] coOwners = ok.Split(',');
            bool fail = coOwners.All(t => t != ID.ToString());
            return !fail;
        }
        public static Task<bool> CheckPermission(ICommandContext context)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    return Task.FromResult(ulong.Parse(botSettingsHelper["ownerID"]) != context.User.Id && context.User.Id != context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id && !IsCoOwner(context.User.Id));
                /*case TokenType.User:
                        return (context.User.Id != context.Client.CurrentUser.Id);*/
                default:
                    return Task.FromResult(false);
            }
        }
        public static Task<ulong> GetOwner(ICommandContext context)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    return Task.FromResult(context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id);
                /*case TokenType.User:
                        return ulong.Parse(config["ownerID"]);*/
                default:
                    return Task.FromResult(0UL);
            }
        }
    }
}
