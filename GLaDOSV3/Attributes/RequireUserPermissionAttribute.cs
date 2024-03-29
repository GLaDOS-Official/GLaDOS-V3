﻿using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GLaDOSV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireUserPermissionAttribute : Discord.Commands.RequireUserPermissionAttribute
    {
        public RequireUserPermissionAttribute(ChannelPermission permission) : base(permission)
        { }
        public RequireUserPermissionAttribute(GuildPermission permission) : base(permission)
        { }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            PreconditionResult result = await base.CheckPermissionsAsync(context, command, services).ConfigureAwait(true);
            if ((!result.IsSuccess && result.Error == CommandError.UnmetPrecondition) && (await IsOwner.CheckPermission(context).ConfigureAwait(true))) return PreconditionResult.FromSuccess();
            return result;
        }
    }
}
