using Discord;
using Discord.Rest;
using Discord.WebSocket;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    internal class IPLoggerProtection
    {
        private readonly DiscordSocketClient discord;
        private readonly List<ulong> serverIds = new List<ulong>() { 658372357924192281, 259776446942150656, 472402015679414293, 503145318372868117, 516296348367192074, 611503265313718282, 611599798595878912, 499598184570421253,  };
        public IPLoggerProtection(DiscordSocketClient discord)
        {
            this.discord = discord;
            this.discord.MessageReceived += this.OnMessageReceivedAsync;
        }
        private readonly string[] knownIpLoggers = { "iplogger", "maper.info", "grabify", "2no.co", "yip.su", "ipgrabber", "iplis.ru", "02ip.ru", "ezstat.ru", "iplo.ru" };

        private async Task DeleteMessage(IMessage msg)
        {
            if (((SocketGuildChannel)msg.Channel).Guild.GetUser(this.discord.CurrentUser.Id).GuildPermissions.ManageMessages) await msg.DeleteAsync().ConfigureAwait(false);
        }

        private Task DeleteMessageDelay(IMessage msg, int delay = 3000)
        {
            Thread deleteThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(delay);
                DeleteMessage(msg);
            });
            deleteThread.Start();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) return; // Ensure the message is from a user/bot
            if (msg.Author.Id == this.discord.CurrentUser.Id) return; // Ignore self when checking commands
            if (msg.Author.IsBot) return; // Ignore other bots
            if (!(msg.Channel is SocketGuildChannel mChanel)) return; // only guild channels please
            if (!this.serverIds.Contains(mChanel.Guild.Id)) return; // private feature :P
            if (!(msg.Content.Contains("http://", StringComparison.Ordinal) || msg.Content.Contains("https://", StringComparison.Ordinal))) return;
            var items = Regex.Matches(msg.Content, @"(http|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
            List<string> urlScanned = new List<string>();
            IdnMapping mapping = new IdnMapping();
            var isIpLogger = false;
            for (var i = 0; i < items.Count; i++)
            {
                if (isIpLogger) return;
                Match item = items[i];
                var shortUrl = item.Value.ToLowerInvariant();
                if (this.knownIpLoggers.Any(var1 => shortUrl.Contains(var1, StringComparison.Ordinal)))
                {
                    isIpLogger = true;
                    await this.DeleteMessage(msg).ConfigureAwait(false);
                    await arg.Channel.SendMessageAsync($"{arg.Author.Mention} Good job! You have sent an IP logger. Message was logged and reported to Trust and Safety team!").ConfigureAwait(false);
                    return; 
                }
                if (urlScanned.Contains(shortUrl))
                    return;
                urlScanned.Add(shortUrl);
                using HttpClient hc = new HttpClient();
                RestUserMessage message = null;
                hc.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                hc.DefaultRequestHeaders.Add("DNT", "1");
                hc.DefaultRequestHeaders.Add("Save-Data", "on");
                hc.DefaultRequestHeaders.Add("Origin", "https://redirectdetective.com");
                hc.DefaultRequestHeaders.Referrer = new Uri("https://redirectdetective.com/");
                hc.DefaultRequestHeaders.Add("User-Agent",
                                             "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                using HttpContent content =
                    new StringContent("w=" + mapping.GetAscii(shortUrl.Replace("http://", "", StringComparison.OrdinalIgnoreCase).Replace("https://", "", StringComparison.OrdinalIgnoreCase)));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                var response = await hc.PostAsync("https://redirectdetective.com/linkdetect.px", content)
                                       .GetAwaiter().GetResult().Content.ReadAsStringAsync().ConfigureAwait(true);
                shortUrl = shortUrl.Replace("%3A", ":", StringComparison.Ordinal);
                shortUrl = shortUrl.Replace("htt", "hxx", StringComparison.OrdinalIgnoreCase);
                var document = new HtmlDocument();
                document.LoadHtml(response);
                var redirectHops = string.Empty;
                var imgNodes = document.DocumentNode.SelectNodes("//img");
                var spanNodes = document.DocumentNode.SelectNodes("//span");
                for (var index = 0; index < imgNodes.Count; index++)
                {
                    var node = imgNodes[index];
                    var text = node.OuterHtml;
                    if (text.Contains("cookie", StringComparison.Ordinal))
                        continue;
                    text = text.Remove(0, 11);
                    text = text.Remove(3);
                    var nodeUrl = spanNodes[(index + 1) / 2].InnerText;
                    var warning = string.Empty;
                    if (this.knownIpLoggers.Any(var1 => nodeUrl.Contains(var1, StringComparison.Ordinal)))
                    {
                        nodeUrl = nodeUrl.Replace("http", "hxxp", StringComparison.OrdinalIgnoreCase);
                        warning = " (KNOWN IP LOGGER!)";
                        isIpLogger = true;
                        await arg.Channel.SendMessageAsync($"{arg.Author.Mention} Good job! You have sent an IP logger. Message was logged and reported to Trust and Safety team!").ConfigureAwait(false);
                        await this.DeleteMessage(msg).ConfigureAwait(false);
                        return;
                    }

                    redirectHops +=
                        $"{nodeUrl.Replace("\n", string.Empty, StringComparison.Ordinal).Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase)} ({text}){warning}\n↓\n";
                }
                if (message == null) continue;
                redirectHops = redirectHops.Remove(redirectHops.Length - 3);
                await this.DeleteMessageDelay(message).ConfigureAwait(false);
            }
        }
    }
}
