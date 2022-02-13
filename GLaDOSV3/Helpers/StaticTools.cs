using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GLaDOSV3.Helpers
{
    public static class StaticTools
    {
        private static readonly Random Rnd = new Random();

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
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOs() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsUnix() => IsLinux() || IsMacOs();
        public static T RandomElement<T>(this T[] items) => items[Rnd.Next(0, items.Length)];
        public static T RandomElement<T>(this List<T> items) => items[Rnd.Next(0, items.Count)];
        public static SocketTextChannel DefaultWritableChannel(this SocketGuild g) => g?.TextChannels.Where(c => (g.CurrentUser.GetPermissions(c).ViewChannel) && g.CurrentUser.GetPermissions(c).SendMessages).OrderBy(c => c.Position).FirstOrDefault();
        public static string ReduceWhitespace(this string value) => Regex.Replace(value, @"\s+", " ", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static EmbedBuilder AddBlankField(this EmbedBuilder builder, bool inline) => builder?.AddField("\u200B", "\u200B", inline);
        /// <summary>Returns the index of an element contained in a list if it is found, otherwise returns -1.</summary>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T element) // IList doesn't implement IndexOf for some reason
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i]?.Equals(element) ?? false) return i;
            return -1;
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string Join<T>(this IEnumerable<T> values, string separator = "") => string.Join(separator, values);
        public static bool Has<T>(this Enum type, T value) => ((int)(object)type & (int)(object)value) == (int)(object)value;

        public static bool Is<T>(this Enum type, T value) => (int)(object)type == (int)(object)value;

        public static T Add<T>(this Enum type, T value) => (T)(object)((int)(object)type | (int)(object)value);

        public static T Remove<T>(this Enum type, T value) => (T)(object)((int)(object)type & ~(int)(object)value);

        public static T Get<T>(this IServiceProvider provider)
        {
            _ = provider ?? throw new ArgumentNullException($"{nameof(provider)} is not initialized!", new NullReferenceException());
            return (T)provider.GetService(typeof(T))!;
        }
        public static bool HasPermission(this SocketGuildUser member, GuildPermission   perm) => member.GuildPermissions.Has(perm);
        public static bool HasPermission(this SocketGuildUser member, GuildPermission[] perm) => perm.Count(permItem => member.GuildPermissions.Has(permItem)) == perm.Length;

        public static bool IsAdministrator(this SocketGuildUser member) => member.GuildPermissions.Administrator || member.Roles.Any(role => role.Permissions.Has(GuildPermission.Administrator));

        public static string GetLastRoleMention(this SocketGuildUser member) => member.Roles.Last().Mention;

        public static bool IsAbove(this SocketGuildUser target, SocketGuildUser comparison) => target.Roles.Any() && target.Hierarchy >= comparison.Hierarchy;
        public static string Center(this string text, string anchor)
        {
            var refLength = anchor.Length;

            if (anchor.Contains('\t')) refLength += anchor.Where(t => t is '\t').Sum(t => 3);

            if (text.Length >= refLength)
                return text;

            var start = (refLength - text.Length) / 2;

            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        public static string Pull(this string text, Range range) =>
            range.End.Value >= text.Length ? text :
            range.Start.Value >= text.Length || range.Start.Value < 0 ? text :
            range.End.IsFromEnd ? text[range] : text[range.Start..Math.Min(text.Length, range.End.Value)];

        public static Stream AsStream(this string s) => new MemoryStream(Encoding.UTF8.GetBytes(s));
        public static SocketUser GetUser(this DiscordSocketClient client, Func<SocketGuildUser, bool> predicate) =>
            client
               .Guilds
               .SelectMany(g => g.Users)
               .FirstOrDefault(predicate);

        public static SocketGuildUser GetUser(this DiscordShardedClient client, Func<SocketGuildUser, bool> predicate) =>
            client
               .Shards
               .SelectMany(c => c.Guilds)
               .SelectMany(g => g.Users)
               .FirstOrDefault(predicate);

        public static async Task DeleteAsync(this IEnumerable<IMessage> messageCollection)
        {
            if (messageCollection is null)
                throw new ArgumentNullException(nameof(messageCollection));

            IEnumerable<IMessage> collection = messageCollection as IMessage[] ?? messageCollection.ToArray();
            SocketTextChannel channel = (SocketTextChannel)collection.First().Channel;
            await channel.DeleteMessagesAsync(collection);
        }
        public static string       GetUrl(this IUser user)   => $"https://discord.com/users/{user.Id}";
        public static List<string> Compare<T>(T x, T y) =>
            (x.GetType()
              .GetFields()
              .Join(y.GetType().GetFields(), l1 => l1.Name, l2 => l2.Name, (l1, l2) => new { l1, l2 })
              .Where(@t => !@t.l1.GetValue(x).Equals(@t.l2.GetValue(y)))
              .Select(@t => $"{@t.l1.Name} {@t.l1.GetValue(x)} {@t.l2.GetValue(y)}")).ToList();

        public static int FindFirstNotOf(this string source, string chars)
        {
            if (source        == null) throw new ArgumentNullException("source");
            if (chars         == null) throw new ArgumentNullException("chars");
            if (source.Length == 0) return -1;
            if (chars.Length  == 0) return 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (chars.IndexOf(source[i]) == -1) return i;
            }
            return -1;
        }

        public static int FindLastNotOf(this string source, string chars)
        {
            if (source        == null) throw new ArgumentNullException("source");
            if (chars         == null) throw new ArgumentNullException("chars");
            if (source.Length == 0) return -1;
            if (chars.Length  == 0) return source.Length - 1;

            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (chars.IndexOf(source[i]) == -1) return i;
            }
            return -1;
        }
    }
}
