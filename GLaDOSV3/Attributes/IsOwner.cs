using Discord;
using Discord.Commands;
using GLaDOSV3.Helpers;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Attributes
{
    public static class IsOwner
    {
        public static BotSettingsHelper<string> botSettingsHelper = new BotSettingsHelper<string>();
        public static bool IsCoOwner(ulong ID)
        {
            string ok = botSettingsHelper["co-owners"];
            if (string.IsNullOrWhiteSpace(ok))
                return false;
            string[] coOwners = ok.Split(',');
            bool fail = coOwners.All(t => t != ID.ToString(CultureInfo.InvariantCulture));
            return !fail;
        }
        public static Task<bool> CheckPermission(ICommandContext context) => context?.Client.TokenType switch
        {
            TokenType.Bot => Task.FromResult(ulong.Parse(botSettingsHelper["ownerID"], NumberStyles.Integer, CultureInfo.InvariantCulture) == context.User.Id || context.User.Id == context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id || IsCoOwner(context.User.Id)),
            _ => Task.FromResult(false),
        };
        public static Task<ulong> GetOwner(ICommandContext context) => context?.Client.TokenType switch
        {
            TokenType.Bot => Task.FromResult(ulong.Parse(botSettingsHelper["ownerID"], NumberStyles.Integer, CultureInfo.InvariantCulture)),
            _ => Task.FromResult(0UL),
        };
    }
}
