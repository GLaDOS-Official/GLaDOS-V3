using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace GladosV3.Module.Default
{
    [Name("Fun")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        [Command("catfact")]
        [Remarks("catfact")]
        [Summary("This is self-explanatory.")]
        public async Task Catfact()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = http.GetAsync("https://catfact.ninja/fact?max_length=2000").GetAwaiter().GetResult()
                    .Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                JObject fact = JObject.Parse(result);
                await ReplyAsync(fact["fact"].Value<string>());
            }
        }
        [Command("illegal")]
        [Remarks("illegal <thing>")]
        [Summary("Did the president banned something again? Powered by IsNowIllegal.com")]
        public async Task Illegal([Remainder]string word)
        {
            if (word.Length < 1 || word.Length > 10) {
                await ReplyAsync("Cannot be longer than 10 characters or shorter than 1 character."); return;
            }
            else if (new Regex("^[a-zA-Z\\s]$").IsMatch(word)) {
                await ReplyAsync("You cannot use non-standard unicode characters."); return;
            }

            var msg = await ReplyAsync("Please wait...");
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                await http.PostAsync("https://is-now-illegal.firebaseio.com/queue/tasks.json",
                    new StringContent(new JObject(new JProperty("task", "gif"), new JProperty("word", word.ToUpper()))
                        .ToString()));
                await Task.Delay(5000);
                var result = http.GetAsync($"https://is-now-illegal.firebaseio.com/gifs/{word.ToUpper()}.json").GetAwaiter()
                    .GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject legal = JObject.Parse(result);
                await msg.ModifyAsync(properties => properties.Content = legal["url"].Value<string>().Replace(" ", "%20"));
            }
        }
        [Command("bunny")]
        [Remarks("bunny")]
        [Summary("Random bunny!")]
        public async Task Bunny()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = http.GetAsync("https://api.bunnies.io/v2/loop/random/?media=gif,poster").GetAwaiter().GetResult()
                    .Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject bunny = JObject.Parse(result);
                await ReplyAsync(
                    $"Here's your bunny! {bunny["media"]["gif"].Value<string>()}");
            }
        }
        [Command("cat")]
        [Remarks("cat")]
        [Summary("Random cat!")]
        public async Task Cat()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-httpd-php"));
                var result = http.GetAsync("http://random.cat/meow").GetAwaiter().GetResult()
                    .Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject cat = JObject.Parse(result);
                await ReplyAsync(
                    $"Here's your cat! {cat["file"].Value<string>()}");
            }
        }
        [Command("dog")]
        [Remarks("dog")]
        [Summary("Random dog!")]
        public async Task Dog()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = http.GetAsync("https://dog.ceo/api/breeds/image/random").GetAwaiter().GetResult()
                    .Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject dog = JObject.Parse(result);
                await ReplyAsync(
                    $"Here's your dog! {dog["message"].Value<string>()}");
            }
        }
    }
}
