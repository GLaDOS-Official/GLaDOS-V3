using System;
using System.Collections.Generic;
using System.Globalization;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    public static class StaticTools
    {
        private static bool _fuckYouDiscord = false;
        private static Random _rnd = new Random();
        public static EmbedBuilder AddBlankField(this EmbedBuilder builder, bool inline) => builder?.AddField("\u200B", "\u200B", inline);
        public static async Task WaitForConnection(this DiscordSocketClient discord)
        {
            discord.Ready += Discord_Connected;
            while (!_fuckYouDiscord) await Task.Delay(100);
            discord.Ready -= Discord_Connected;
            await Task.Delay(100); //are you done discord??
        }

        private static async Task Discord_Connected() => _fuckYouDiscord = true;
        public static Task<string> FormatText(this SocketGuildUser user, string text) =>
            Task.FromResult(text.Replace("{mention}", user.Mention, StringComparison.Ordinal)
                                .Replace("{uname}", user.Username, StringComparison.Ordinal)
                                .Replace("{sname}", user.Guild.Name, StringComparison.Ordinal)
                                .Replace("{count}", user.Guild.MemberCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                                .Replace("{ucreatedate}", user.CreatedAt.ToString("MM-dd-yy"))
                                .Replace("{udiscrim}", user.Discriminator)
                                .Replace("{unick}", user.Nickname));

        public static Task<string> FormatText(this SocketUser user, string text) =>
            Task.FromResult(text.Replace("{mention}", user.Mention, StringComparison.Ordinal)
                                .Replace("{uname}", user.Username, StringComparison.Ordinal)
                                .Replace("{ucreatedate}", user.CreatedAt.ToString("MM-dd-yy"))
                                .Replace("{udiscrim}", user.Discriminator));
        public static T RandomElement<T>(this T[] items) => items[_rnd.Next(0, items.Length)];
        public static T RandomElement<T>(this List<T> items) => items[_rnd.Next(0, items.Count)];
        public static SocketTextChannel DefaultWritableChannel(this SocketGuild g) => g?.TextChannels.Where(c => (g.CurrentUser.GetPermissions(c).ViewChannel) && g.CurrentUser.GetPermissions(c).SendMessages).OrderBy(c => c.Position).FirstOrDefault();
        public static String ReduceWhitespace(this String value) => Regex.Replace(value, @"\s+", " ", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
}
