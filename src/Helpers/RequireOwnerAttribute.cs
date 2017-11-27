using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;

namespace GladosV3.Helpers
{
    public class RequireOwnerAttribute : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Get the ID of the bot's owner

            var ownerId = (await context.Client.GetApplicationInfoAsync()).Owner.Id;
            // If this command was executed by that user, return a success
            if (context.User.Id == ownerId)
                return PreconditionResult.FromSuccess();
            // Since it wasn't, fail
            else
                return PreconditionResult.FromError("You must be the owner of the bot to run this command.");
        }
    }

}
