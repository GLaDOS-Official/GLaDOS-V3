using Discord.Commands;
using GLaDOSV3.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GLaDOSV3.Module.Default
{
    [Name("NSFW")]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        [Attributes.RequireOwner]
        [RequireContext(ContextType.Guild)]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("nsfw enable")]
            [Remarks("nsfw enable")]
            [Summary("Enables nsfw module (disabled by default)")]
            public async Task Enable()
            {
                await SqLite.Connection.SetValueAsync("servers", "nsfw", 1, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                await this.ReplyAsync("The nsfw has been enabled!").ConfigureAwait(false);
            }
            [Command("nsfw disable")]
            [Remarks("nsfw disable")]
            [Summary("Disables nsfw module (disabled by default)")]
            public async Task Disable()
            {
                await SqLite.Connection.SetValueAsync("servers", "nsfw", 0, $"WHERE guildid={Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}").ConfigureAwait(false);
                await this.ReplyAsync("The nsfw has been disabled!").ConfigureAwait(false);
            }
            [Command("nsfw status")]
            [Remarks("nsfw status")]
            [Summary("Get's status of the nsfw module (disabled by default)")]
            public async Task Status()
            {
                string result = (Convert.ToInt32(SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}'").GetAwaiter().GetResult().Rows[0]["nsfw"], CultureInfo.InvariantCulture) == 1) ? "enabled" : "disabled";
                var message =
                    $"The current status of nsfw module is: {result}";
                await this.ReplyAsync(message).ConfigureAwait(false);
            }

        }

        private readonly string[] blacklisted_tags = { "GORE", "LOLI", "SHOTA" };
        [Command("e621")]
        [Remarks("e621 [tags]")]
        [Summary("Find images on e621 by the given tags.")]
        [RequireNsfw]
        public async Task E621([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}'").GetAwaiter().GetResult().Rows[0]["nsfw"], CultureInfo.InvariantCulture) == 0)
            { await this.ReplyAsync("The nsfw module is disabled on this server!").ConfigureAwait(false); return; }
            string url = "https://e621.net/post/index.json?limit=20";
            if (!string.IsNullOrEmpty(tags))
                url += $"&tags={string.Join(" ", tags)}";
            if (this.blacklisted_tags.Any(tags.ToUpper(CultureInfo.InvariantCulture).Contains))
            { await this.ReplyAsync("You should probadly read the discord TOS...").ConfigureAwait(false); return; }
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            var httpResult = http.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                .GetResult();

            JArray images = JArray.Parse(httpResult);
            if (images.Count == 0)
            { await this.ReplyAsync("Couldn't find an image with those tags.").ConfigureAwait(false); return; }

            string ext = string.Empty;
            JObject image = null;
            int retries = 6;
            while (ext != "png" && ext != "jpg" && ext != "jpeg" && ext != "gif" && ext != "webm" && ext != "mp4" && retries >= 0)
            {
                image = (JObject)images[new Random().Next(0, images.Count)];
                ext = image.GetValue("file_ext", StringComparison.OrdinalIgnoreCase).ToObject<string>();
                retries--;
            }
            if (image == null)
            { await this.ReplyAsync("Couldn't find an image with those tags.").ConfigureAwait(false); return; }
            await this.ReplyAsync($"Image score: {image.GetValue("score", StringComparison.OrdinalIgnoreCase).ToObject<string>()}\n{image.GetValue("file_url", StringComparison.OrdinalIgnoreCase).ToObject<string>()}").ConfigureAwait(false);
        }
        [Command("r34")]
        [Remarks("r34 [tags]")]
        [Summary("Find images on rule34 by the given tags.")]
        [RequireNsfw]
        public async Task Rule34([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{Context.Guild.Id.ToString(CultureInfo.InvariantCulture)}'").GetAwaiter().GetResult().Rows[0]["nsfw"], CultureInfo.InvariantCulture) == 0)
            { await this.ReplyAsync("The nsfw module is disabled on this server!").ConfigureAwait(false); return; }
            string url = "https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=20";
            if (!string.IsNullOrEmpty(tags))
                url += $"&tags={string.Join(" ", tags)}";
            if (this.blacklisted_tags.Any(tags.ToUpper(CultureInfo.InvariantCulture).Contains))
            { await this.ReplyAsync("You should probadly read the discord TOS...").ConfigureAwait(false); return; }
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            var httpResult = http.GetAsync(new Uri(url)).ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                .GetResult();
            var xml = XDocument.Parse(httpResult);
            if (xml.Root != null)
            {
                var xNodes = xml.Root.Elements().ToList();
                var rndNode = xNodes[new Random().Next(xNodes.Count - 1)];
                await this.ReplyAsync($"Image score: {rndNode?.Attribute("score")?.Value}\n{rndNode?.Attribute("file_url")?.Value}").ConfigureAwait(false);
            }
        }
    }
}
