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
    public class IPLoggerProtection
    {
        private readonly DiscordSocketClient _discord;
        private readonly List<ulong> serverIds = new List<ulong>() { 259776446942150656, 472402015679414293, 503145318372868117, 516296348367192074, 611503265313718282, 611599798595878912, 499598184570421253 };
        private readonly bool silentMessage = true;
        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public IPLoggerProtection(DiscordSocketClient discord)
        {
            this._discord = discord;
            this._discord.MessageReceived += this.OnMessageReceivedAsync;
        }
        private readonly string[] knownIpLoggers = new string[] { "iplogger", "maper.info", "grabify", "iplogger.org", "2no.co", "yip.su", "ipgrabber", "iplis.ru", "02ip.ru", "ezstat.ru", "iplo.ru" };

        private async Task DeleteMessage(SocketUserMessage msg)
        {
            if (((SocketGuildChannel)msg.Channel).Guild.GetUser(this._discord.CurrentUser.Id).GuildPermissions.ManageMessages)
                await msg.DeleteAsync();
        }

        private Task DeleteMessageDelay(RestUserMessage msg, int delay = 3000)
        {
            Thread deleteThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(delay);
                msg.DeleteAsync().GetAwaiter();
            });
            deleteThread.Start();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) return; // Ensure the message is from a user/bot
            if (msg.Author.Id == this._discord.CurrentUser.Id) return; // Ignore self when checking commands
            if (msg.Author.IsBot) return; // Ignore other bots
            if (!(msg.Channel is SocketGuildChannel mChanel)) return; // only guild channels please
            if (!this.serverIds.Contains(mChanel.Guild.Id)) return; // private feature :P
            if (!(msg.Content.Contains("http://") || msg.Content.Contains("https://"))) return;
            var items = Regex.Matches(msg.Content,
                @"(http|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
            List<string> urlScanned = new List<string>();
            IdnMapping mapping = new IdnMapping();
            bool isIpLogger = false;
            for (var i = 0; i < items.Count; i++)
            {
                if (isIpLogger) return;
                Match item = items[i];
                string shortUrl = item.Value;
                if (this.knownIpLoggers.Any(var1 => shortUrl.Contains(var1)))
                {
                    isIpLogger = true;
                    await this.DeleteMessage(msg);
                    await arg.Channel.SendMessageAsync(
                        $"{arg.Author.Mention} Good job! You have sent an IP logger. Message was logged and reported to Trust and Safety team!");
                    if (urlScanned.Contains(shortUrl))
                        return;
                    urlScanned.Add(shortUrl);
                    using HttpClient hc = new HttpClient();
                    RestUserMessage message = null;
                    if (!this.silentMessage)
                        message = await msg.Channel.SendMessageAsync(
                            $"[{i + 1}]Verifying URL from {msg.Author.Username}#{msg.Author.Discriminator}...");
                    hc.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                    hc.DefaultRequestHeaders.Add("DNT", "1");
                    hc.DefaultRequestHeaders.Add("Save-Data", "on");
                    hc.DefaultRequestHeaders.Add("Origin", "https://redirectdetective.com");
                    hc.DefaultRequestHeaders.Referrer = new Uri("https://redirectdetective.com/");
                    hc.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                    HttpContent content =
                        new StringContent(
                            "w=" + mapping.GetAscii(shortUrl.Replace("http://", "").Replace("https://", "")));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                    var response = await hc.PostAsync("https://redirectdetective.com/linkdetect.px", content)
                        .GetAwaiter().GetResult().Content.ReadAsStringAsync();
                    shortUrl = shortUrl.Replace("%3A", ":");
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"Posted by {msg.Author.Username}#{msg.Author.Discriminator}",
                            IconUrl = (msg.Author.GetAvatarUrl())
                        }
                    };
                    if (isIpLogger)
                        shortUrl = shortUrl.Replace("htt", "hxx");
                    if (response == "<h4>No redirects found</h4>")
                    {
                        builder.Color = isIpLogger ? Color.DarkRed : Color.Green;
                        builder.AddField($"Hops",
                            isIpLogger ? "Known IP logger found, no hops." : "Not an IP logger, no hops.");
                        builder.AddField("Original URL", shortUrl);
                        if (!this.silentMessage)
                            await message.ModifyAsync((a) =>
                            {
                                a.Embed = builder.Build();
                                a.Content = "";
                            });
                        else
                            message = await msg.Channel.SendMessageAsync(embed: builder.Build());
                        await this.DeleteMessageDelay(message, 3000);
                        continue;
                    }

                    var document = new HtmlDocument();
                    document.LoadHtml(response);
                    var redirectHops = string.Empty;
                    var imgNodes = document.DocumentNode.SelectNodes("//img");
                    var spanNodes = document.DocumentNode.SelectNodes("//span");
                    for (var index = 0; index < imgNodes.Count; index++)
                    {
                        var node = imgNodes[index];
                        var text = node.OuterHtml;
                        if (text.Contains("cookie"))
                            continue;
                        text = text.Remove(0, 11);
                        text = text.Remove(3);
                        var nodeUrl = spanNodes[index / 2].InnerText;
                        string warning = "";
                        if (this.knownIpLoggers.Any(var1 => nodeUrl.Contains(var1)))
                        {
                            nodeUrl = nodeUrl.Replace("htt", "hxx");
                            warning = " (KNOWN IP LOGGER!)";
                            if (!isIpLogger)
                            {
                                isIpLogger = true;
                                await this.DeleteMessage(msg);
                                await arg.Channel.SendMessageAsync(
                                    $"{arg.Author.Mention} Good job! You have sent an IP logger. Message was logged and reported to Trust and Safety team!");
                            }
                        }

                        redirectHops +=
                            $"{nodeUrl.Replace("\n", string.Empty).Replace("\r", string.Empty)} ({text}){warning}\n↓\n";
                    }

                    if (message == null) continue;
                    redirectHops = redirectHops.Remove(redirectHops.Length - 3);
                    builder.AddField($"Hops", redirectHops);
                    builder.AddField("Original URL", shortUrl);
                    builder.Color = isIpLogger ? Color.DarkRed : Color.Green;
                    await message.ModifyAsync((a) =>
                    {
                        a.Embed = builder.Build();
                        a.Content = "";
                    });
                    await this.DeleteMessageDelay(message);
                }
            }
        }
    }
}
