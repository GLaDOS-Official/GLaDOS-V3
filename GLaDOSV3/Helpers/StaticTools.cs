using System;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GladosV3.Helpers
{
    public static class StaticTools
    {
        private static bool fuckYouDiscord = false;
        public static EmbedBuilder AddBlankField(this EmbedBuilder builder, bool inline) => builder?.AddField("\u200B", "\u200B", inline);
        public static async Task WaitForConnection(this DiscordSocketClient discord)
        {
            discord.Ready += Discord_Connected;
            while (!fuckYouDiscord) await Task.Delay(100);
            discord.Ready -= Discord_Connected;
            await Task.Delay(100); //are you done discord??
        }

        private static async Task Discord_Connected() => fuckYouDiscord = true;

        public static SocketTextChannel DefaultWritableChannel(this SocketGuild g) => g?.TextChannels.Where(c => (g.CurrentUser.GetPermissions(c).ViewChannel) && g.CurrentUser.GetPermissions(c).SendMessages).OrderBy(c => c.Position).FirstOrDefault();
        public static String ReduceWhitespace(this String value) => Regex.Replace(value, @"\s+", " ", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
}
