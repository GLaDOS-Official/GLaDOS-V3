using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    public sealed class Tools
    {
        public static int RoundToDividable<T>(int number, int dividable) => (int)RoundToDividable<double>((double)number, dividable);
        public static double RoundToDividable<T>(double number, double dividable) => Math.Ceiling(number / dividable) * dividable;
        private static Random rnd = new Random();
        public static async Task<string> EscapeMentionsAsync(IGuild g, IChannel channel, string message)
        {
            if (message == null || channel == null || message == null) return null;
            message = message.Replace("@here", "@\x200bhere", StringComparison.Ordinal)
                             .Replace("@everyone", "@\x200beveryone", StringComparison.Ordinal);
            Regex r = new Regex("<@&?!?(\\d+)>", RegexOptions.Compiled);
            foreach (Match m in r.Matches(message))
            {
                if (!m.Success) continue;
                if (m.Captures.Count == 0) continue;
                var id = m.Groups[1].Value;
                if (!ulong.TryParse(id, out var uId)) continue;
                if (m.Groups[0].Value.Contains('&', StringComparison.Ordinal))
                {
                    var role = g?.GetRole(uId);
                    if (role == null) continue;
                    message = message.Replace(m.Groups[0].Value, $"@\x200b{role.Name}", StringComparison.Ordinal);
                }
                else
                {
                    var user = g == null ? await channel.GetUserAsync(uId).ConfigureAwait(true) : await g.GetUserAsync(uId).ConfigureAwait(true);
                    if (user == null) continue;
                    message = message.Replace(m.Groups[0].Value, $"@\x200b{user.Username}#{user.Discriminator}", StringComparison.Ordinal);
                }
            }
            return message;
        }
        private static readonly object _writeLock = new object();
        public static void WriteColorLine(ConsoleColor color, string message)
        {
            lock (_writeLock)
            {
                var fcolor = Console.ForegroundColor;
                var bcolor = Console.BackgroundColor;
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Out.WriteLine(message);
                Console.ForegroundColor = fcolor;
                Console.BackgroundColor = bcolor;
            }
        }

        public static void WriteColor(ConsoleColor color, string message)
        {
            lock (_writeLock)
            {
                var fcolor = Console.ForegroundColor;
                var bcolor = Console.BackgroundColor;
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Out.Write(message);
                Console.ForegroundColor = fcolor;
                Console.BackgroundColor = bcolor;
            }
        }
        public static void RestartApp()
        {
            using var proc = new Process
            {
                StartInfo =
                {
                    FileName = "dotnet",
                    Arguments = $"{Assembly.GetEntryAssembly()?.Location}",
                    UseShellExecute = false,
                    WorkingDirectory = AppContext.BaseDirectory
                }
            };
            proc.Start();
            Environment.Exit(1);
        }
        public static async Task<dynamic> GetProxyAsync()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://gimmeproxy.com/api/getProxy?anonymityLevel=1&user-agent=true&protocol=http&country=GB,CZ,DE,SK,FR&minSpeed=1024"));
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            string json;
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(true))
            {
                await using Stream stream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException());
                json = await reader.ReadToEndAsync().ConfigureAwait(true);
            }
            if (string.IsNullOrWhiteSpace(json)) return await Task.FromResult("").ConfigureAwait(false);
            var Object = JObject.Parse(json);
            return await Task.FromResult(new WebProxy { Address = new Uri($"http://{Object["ipPort"]}") }).ConfigureAwait(false);
        }
        internal static void ReleaseMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            if (SqLite.Connection != null && SqLite.Connection.State != System.Data.ConnectionState.Closed)
                SqLite.Connection.ReleaseMemory();
        }
        public static string[] SplitMessage(string message, int len) // discord.js :D
        {
            if (message?.Length <= len) return new[] { message };
            var splitText = message.Split('\n');
            if (splitText.Length == 1) throw new Exception("SPLIT_MAX_LEN");
            List<string> messages = new List<string>();
            var msg = string.Empty;
            foreach (var chunk in splitText)
            {
                if (($"{msg}\n{chunk}").Length > len)
                {
                    messages.Add(msg);
                    msg = string.Empty;
                }

                msg += $"{(!string.IsNullOrEmpty(msg) ? "\n" : "")}{chunk}";
            }

            messages.Add(msg);
            return messages.ToArray();
        }

        public static bool WriteToReadOnlyValue(Type type, object obj, string element, object value)
        {
            PropertyInfo[] array = type.GetProperties();
            foreach (var info in array)
            {
                if (info.Name != element) continue;
                if (!info.CanWrite) return false;
                info.SetValue(obj, value);
                return true;
            }
            return false;
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
