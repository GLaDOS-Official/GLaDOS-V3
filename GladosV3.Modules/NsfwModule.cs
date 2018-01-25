using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Discord.Commands;
using GladosV3.Attributes;
using GladosV3.Helpers;
using Newtonsoft.Json.Linq;

namespace GladosV3.Modules
{
    [Name("NSFW")]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        [Group("NSFW")]
        [Attributes.RequireOwner]
        [RequireContext(ContextType.Guild)]
        public class Bot : ModuleBase<SocketCommandContext>
        {
            [Command("enable")]
            [Remarks("nsfw enable")]
            [Summary("Enables nsfw module (disabled by default)")]
            public async Task Enable()
            {
                SqLite.Connection.SetValue("servers", "nsfw", 1, Context.Guild.Id.ToString());
                await ReplyAsync("The nsfw has been enabled!");
            }
            [Command("disable")]
            [Remarks("nsfw disable")]
            [Summary("Disables nsfw module (disabled by default)")]
            public async Task Disable()
            {
                SqLite.Connection.SetValue("servers","nsfw",0, Context.Guild.Id.ToString());
                await ReplyAsync("The nsfw has been disabled!");
            }
            [Command("status")]
            [Remarks("nsfw status")]
            [Summary("Get's status of the nsfw module (disabled by default)")]
            public async Task Status()
            {
                string result = (Convert.ToInt32(SqLite.Connection.GetValues("servers", Context.Guild.Id.ToString()).Rows[0]["nsfw"]) == 1) ? "enabled" : "disabled";
                var message =
                    $"The current status of nsfw module is: {result}";
                await ReplyAsync(message);
            }

        }

        [Command("e621")]
        [Remarks("e621 [tags]")]
        [Summary("Find images on e621 by the given tags.")]
        [RequireNsfw]
        public async Task E621([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValues("servers", Context.Guild.Id.ToString()).Rows[0]["nsfw"]) == 0)
            {
                await ReplyAsync("The nsfw module is disabled on this server!");
                return;
            }
            string url = "https://e621.net/post/index.json?limit=20";
            if (tags != "")
                url += String.Format("&tags={0}", string.Join(" ", tags));
            if (tags.ToUpper().Contains("GORE"))
            {
                await ReplyAsync("What is wrong with you? Seriously? Gore?");
                return;
            }
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                var httpResult = http.GetAsync(url).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult();

                JArray images = JArray.Parse(httpResult);
                if (images.Count == 0)
                {
                    await ReplyAsync("Couldn't find an image with those tags.");
                    return;
                }

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
                {
                    await ReplyAsync("Couldn't find an image with those tags.");
                    return;
                }
                await ReplyAsync($"Score: {image.GetValue("score").ToObject<string>()}{image.GetValue("file_url").ToObject<string>()}");
            }
        }
        [Command("r34")]
        [Remarks("r34 [tags]")]
        [Summary("Find images on rule34 by the given tags.")]
        [RequireNsfw]
        public async Task Rule34([Remainder]string tags = "")
        {
            if (Convert.ToInt32(SqLite.Connection.GetValues("servers", Context.Guild.Id.ToString()).Rows[0]["nsfw"]) == 0)
            {
                await ReplyAsync("The nsfw module is disabled on this server!");
                return;
            }
            string url = "https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=20";
            if (tags != "")
                url += String.Format("&tags={0}", string.Join(" ", tags));
            if (tags.ToUpper().Contains("GORE"))
            {
                await ReplyAsync("What is wrong with you? Seriously? Gore?");
                return;
            }
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                var httpResult = http.GetAsync(url).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                var xml = XDocument.Parse(httpResult);
                if (xml.Root != null)
                {
                    var xNodes = xml.Root.Elements().ToList();
                    var rndNode = xNodes[new Random().Next(xNodes.Count - 1)];
                    await ReplyAsync($"Score: {rndNode?.Attribute("score")?.Value}\nhttps:{rndNode?.Attribute("file_url")?.Value}");
                }
            }
        }
    }
}
