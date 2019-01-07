using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord.Commands;
using GladosV3.Helpers;
using Newtonsoft.Json.Linq;

namespace GladosV3.Module.Default
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
                await SqLite.Connection.SetValueAsyncWithGuildFiltering("servers", "nsfw", 1, Context.Guild.Id.ToString()).ConfigureAwait(false);
                await ReplyAsync("The nsfw has been enabled!");
            }
            [Command("nsfw disable")]
            [Remarks("nsfw disable")]
            [Summary("Disables nsfw module (disabled by default)")]
            public async Task Disable()
            {
                await SqLite.Connection.SetValueAsyncWithGuildFiltering("servers","nsfw",0, Context.Guild.Id.ToString()).ConfigureAwait(false);
                await ReplyAsync("The nsfw has been disabled!");
            }
            [Command("nsfw status")]
            [Remarks("nsfw status")]
            [Summary("Get's status of the nsfw module (disabled by default)")]
            public async Task Status()
            {
                string result = (Convert.ToInt32(SqLite.Connection.GetValuesAsyncWithGuildIDFilter("servers", Context.Guild.Id.ToString()).GetAwaiter().GetResult().Rows[0]["nsfw"]) == 1) ? "enabled" : "disabled";
                var message =
                    $"The current status of nsfw module is: {result}";
                await ReplyAsync(message);
            }

        }
        string[] blacklisted_tags = { "GORE","LOLI","SHOTA" };
        [Command("e621")]
        [Remarks("e621 [tags]")]
        [Summary("Find images on e621 by the given tags.")]
        [RequireNsfw]
        public async Task E621([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValuesAsyncWithGuildIDFilter("servers", Context.Guild.Id.ToString()).GetAwaiter().GetResult().Rows[0]["nsfw"]) == 0)
            { await ReplyAsync("The nsfw module is disabled on this server!"); return; }
            string url = "https://e621.net/post/index.json?limit=20";
            if (tags != "")
                url += $"&tags={string.Join(" ", tags)}";
            if (blacklisted_tags.Any(tags.ToUpper().Contains))
            { await ReplyAsync("You should probadly read the discord TOS..."); return; }
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                var httpResult = http.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult();

                JArray images = JArray.Parse(httpResult);
                if (images.Count == 0)
                { await ReplyAsync("Couldn't find an image with those tags."); return; }

                string ext = String.Empty;
                JObject image = null;
                int retries = 6;
                while (ext != "png" && ext != "jpg" && ext != "jpeg" && ext != "gif" && ext != "webm" && ext != "mp4" && retries >= 0)
                {
                    image = (JObject)images[new Random().Next(0, images.Count)];
                    ext = image.GetValue("file_ext").ToObject<string>();
                    retries--;
                }
                if (image == null)
                { await ReplyAsync("Couldn't find an image with those tags."); return; }
                await ReplyAsync($"Image score: {image.GetValue("score").ToObject<string>()}\n{image.GetValue("file_url").ToObject<string>()}");
            }
        }
        [Command("r34")]
        [Remarks("r34 [tags]")]
        [Summary("Find images on rule34 by the given tags.")]
        [RequireNsfw]
        public async Task Rule34([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValuesAsyncWithGuildIDFilter("servers", Context.Guild.Id.ToString()).GetAwaiter().GetResult().Rows[0]["nsfw"]) == 0)
            {  await ReplyAsync("The nsfw module is disabled on this server!"); return; }
            string url = "https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=20";
            if (tags != "")
                url += $"&tags={string.Join(" ", tags)}";
            if (blacklisted_tags.Any(tags.ToUpper().Contains))
            { await ReplyAsync("You should probadly read the discord TOS..."); return; }
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                var httpResult = http.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                var xml = XDocument.Parse(httpResult);
                if (xml.Root != null)
                {
                    var xNodes = xml.Root.Elements().ToList();
                    var rndNode = xNodes[new Random().Next(xNodes.Count - 1)];
                    await ReplyAsync($"Image score: {rndNode?.Attribute("score")?.Value}\n{rndNode?.Attribute("file_url")?.Value}");
                }
            }
        }
    }
}
