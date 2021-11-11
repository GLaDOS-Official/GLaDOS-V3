using Discord;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    public sealed class Tools
    {
        internal static Task LogAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    if (msg.Exception == null)
                        Log.Fatal(msg.Message);
                    else
                        Log.Fatal(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Error:
                    if (msg.Exception == null)
                        Log.Error(msg.Message);
                    else
                        Log.Error(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Warning:
                    if (msg.Exception == null)
                        Log.Warning(msg.Message);
                    else
                        Log.Warning(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Info:
                    if (msg.Exception == null)
                        Log.Information(msg.Message);
                    else
                        Log.Information(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Verbose:
                    if (msg.Exception == null)
                        Log.Verbose(msg.Message);
                    else
                        Log.Verbose(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Debug:
                    if (msg.Exception == null)
                        Log.Debug(msg.Message);
                    else
                        Log.Debug(msg.Exception, msg.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return Task.CompletedTask;
        }
        public static int RoundToDividable<T>(int number, int dividable) => (int)RoundToDividable<double>((double)number, dividable);
        public static double RoundToDividable<T>(double number, double dividable) => Math.Ceiling(number / dividable) * dividable;
        private static readonly Random _rnd = new Random();
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
        
        public static void RestartApp()
        {
            using var proc = new Process
            {
                StartInfo =
                {
                    FileName         = "dotnet",
                    Arguments        = $"{Environment.ProcessPath}",
                    UseShellExecute  = false,
                    WorkingDirectory = AppContext.BaseDirectory
                }
            };
            proc.Start();
            Environment.Exit(1);
        }
        public static async Task<dynamic> GetProxyAsync()
        {
            HttpClient client = new HttpClient();
            string json;
            await using Stream stream = await client.GetStreamAsync(new Uri("http://gimmeproxy.com/api/getProxy?anonymityLevel=1&user-agent=true&protocol=http&country=GB,CZ,DE,SK,FR&minSpeed=1024")).ConfigureAwait(true);
            using StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException());
            json = await reader.ReadToEndAsync().ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(json)) return await Task.FromResult("").ConfigureAwait(false);
            var @object = JObject.Parse(json);
            return await Task.FromResult(new WebProxy { Address = new Uri($"http://{@object["ipPort"]}") }).ConfigureAwait(false);
        }
        internal static void ReleaseMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            if (SqLite.Connection == null || SqLite.Connection.State == System.Data.ConnectionState.Closed)
                return;
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
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[_rnd.Next(s.Length)]).ToArray());
        }
    }
}
