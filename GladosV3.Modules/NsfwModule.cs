using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Discord.Commands;
using GladosV3.Helpers;
using Newtonsoft.Json.Linq;

namespace GladosV3.Modules
{
    [Name("NSFW")]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        [NsfwAttribute] // https://i.gyazo.com/b0d574457b7b785abe0eae1f8a954729.png
        [Command("e621")]
        [Summary("e621 [tags]")]
        [Remarks("Find images on e621 by the given tags.")]
        public async Task E621([Remainder]string tags = "")
        {
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
                int retries = 5;
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
        [NsfwAttribute]
        [Command("r34")]
        [Summary("r34 [tags]")]
        [Remarks("Find images on rule34 by the given tags.")]
        public async Task Rule34([Remainder]string tags = "")
        {
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
                    Random rnd = new Random();
                    var rndNode = xNodes[rnd.Next(xNodes.Count - 1)];
                    await ReplyAsync($"Score: {rndNode?.Attribute("score")?.Value}\nhttps:{rndNode?.Attribute("file_url")?.Value}");
                }
            }
            //
        }
    }
}
