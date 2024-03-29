﻿using Discord;
using Discord.Commands;
using GLaDOSV3.Helpers;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Attributes
{
    public static class IsOwner
    {
        public static BotSettingsHelper<string> BotSettingsHelper = new BotSettingsHelper<string>();
        public static bool IsCoOwner(ulong id)
        {
            var ok = BotSettingsHelper["co-owners"];
            if (string.IsNullOrWhiteSpace(ok))
                return false;
            var coOwners = ok.Split(',');
            var fail = coOwners.All(t => t != id.ToString(CultureInfo.InvariantCulture));
            return !fail;
        }
        public static Task<bool> CheckPermission(ICommandContext context) => context?.Client.TokenType switch
        {
            TokenType.Bot => Task.FromResult(ulong.Parse(BotSettingsHelper["ownerID"], NumberStyles.Integer, CultureInfo.InvariantCulture) == context.User.Id || context.User.Id == context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id || IsCoOwner(context.User.Id)),
            _ => Task.FromResult(false),
        };
    }
}
