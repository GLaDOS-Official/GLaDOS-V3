using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GladosV3.Modules
{
    [Name("General")]
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("choose")]
        [Summary("Returns a random item that you supplied (splitting by comma character)")]
        [Remarks("choose <items>")]
        [Alias("random")]
        public async Task Choose([Remainder]string text)
        {
            string[] array = text.Split(',');
            Random rnd = new Random();
            await ReplyAsync($"I have chosen: {array[rnd.Next(array.Length - 1)]}");
        }

        [Command("strawpoll")]
        [Summary("Creates a strawpoll on strawpoll.me (splitting by comma character)")]
        [Remarks("strawpoll <title> | <options>")]
        [Alias("poll")]
        public async Task Strawpoll([Remainder] string text)
        {
            string[] array = text.Split('|');
            string[] options = array[1].Split(',');
            if (array.Length >= 30)
            {
                await ReplyAsync("The maximum number of options is 30.");
                return;
            }

            using (var http = new HttpClient())
            {
                JObject classO = new JObject(new JProperty("title", array.First()),new JProperty("options", new JArray(options)), new JProperty("multi", false),new JProperty("captcha",true));
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                var httpResult = await http
                    .PostAsync("http://www.strawpoll.me/api/v2/polls",
                        new StringContent(classO.ToString(), Encoding.UTF8, "application/json"));
                httpResult.EnsureSuccessStatusCode();
                JObject response = JObject.Parse(httpResult.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                await ReplyAsync(
                    $"I have created the strawpoll that you wanted. Here's the url: https://www.strawpoll.me/{response["id"]}");
            }
        }
    }
}
