using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace GladosV3.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("_configuration.json").Build();
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync();
                    if (context.User.Id != application.Owner.Id && Convert.ToUInt64(config["ownerID"]) != context.User.Id)
                        return PreconditionResult.FromError("Command can only be run by the owner of the bot");
                    return PreconditionResult.FromSuccess();
                case TokenType.User:
                    if (context.User.Id != context.Client.CurrentUser.Id)
                        return PreconditionResult.FromError("Command can only be run by the owner of the bot");
                    return PreconditionResult.FromSuccess();
                default:
                    return PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }

    public class IsOwner
    {
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public IsOwner(
            IConfigurationRoot config)
        {
            _config = config;
        }
        public async Task<bool> CheckPermission(ICommandContext context, CommandInfo command, IServiceProvider services, IConfigurationRoot config)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync();
                    if (context.User.Id == application.Owner.Id ||
                        Convert.ToUInt64(_config["ownerID"]) == context.User.Id) return true;
                    return false;
                case TokenType.User:
                    if (context.User.Id != context.Client.CurrentUser.Id)
                        return false;
                    return true;
                default:
                    return false;
            }
        }
    }
}
