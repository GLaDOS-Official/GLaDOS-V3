using Discord;
using Discord.Commands;
using GladosV3.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GladosV3.Module.Default
{
    [Name("Fun")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private Random rnd = new Random();
        [Command("catfact")]
        [Remarks("catfact")]
        [Summary("This is self-explanatory.")]
        public async Task Catfact()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = http.GetAsync($"https://catfact.ninja/fact?max_length=2000").GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter()
                .GetResult();
            JObject fact = JObject.Parse(result);
            await ReplyAsync(fact["fact"].Value<string>());
        }
        [Command("illegal")]
        [Remarks("illegal <thing>")]
        [Summary("Did the president banned something again? Powered by IsNowIllegal.com")]
        [Timeout(2, 45, Measure.Seconds)]
        public async Task Illegal([Remainder]string word)
        {
            if (!new Regex("^[a-zA-Z\\s]{0,10}$").IsMatch(word))
            {
                await ReplyAsync("You cannot use non-standard unicode characters and it cannot be longer than 10 characters!"); return;
            }

            var msg = await ReplyAsync("Please wait...");
            using var http = new HttpClient();
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
        [Command("bunny")]
        [Remarks("bunny")]
        [Summary("Random bunny!")]
        [Timeout(3, 15, Measure.Seconds)]
        public async Task Bunny()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = http.GetAsync("https://api.bunnies.io/v2/loop/random/?media=gif,poster,mp4").GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JObject bunny = JObject.Parse(result);
            await ReplyAsync(
                $"Here's your bunny! {bunny["media"]["gif"].Value<string>()}");
        }
        [Command("cat")]
        [Remarks("cat")]
        [Summary("Random cat!")]
        [Timeout(3, 15, Measure.Seconds)]
        public async Task Cat()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-httpd-php"));
            var result = http.GetAsync("http://aws.random.cat/meow").GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JObject cat = JObject.Parse(result);
            await ReplyAsync(
                $"Here's your cat! {cat["file"].Value<string>()}");
        }
        [Command("dog")]
        [Remarks("dog")]
        [Summary("Random dog!")]
        [Timeout(3, 15, Measure.Seconds)]
        public async Task Dog()
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = http.GetAsync("https://dog.ceo/api/breeds/image/random").GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JObject dog = JObject.Parse(result);
            await ReplyAsync(
                $"Here's your dog! {dog["message"].Value<string>()}");
        }
        [Command("urban")]
        [Remarks("urban")]
        [Summary("Search the urban dictionary!")]
        [Timeout(3, 15, Measure.Seconds)]
        public async Task Urban([Remainder] string word)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={word}").ConfigureAwait(false);
            JObject dictionary = JObject.Parse(result);
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = (Context.User.GetAvatarUrl())
                }
            };
            if (!dictionary["list"].HasValues)
            {
                builder.AddField(x =>
                {
                    x.Name = "**Search**";
                    x.IsInline = false;
                    x.Value = word;
                });
                builder.AddField(x =>
                {
                    x.Name = "**Definition**";
                    x.IsInline = false;
                    x.Value = "Not found";
                });
                await Context.Channel.SendMessageAsync(embed: builder.Build());
                return;
            }
            builder.AddField(x =>
            {
                x.Name = "**Search**";
                x.IsInline = false;
                x.Value = word;
            });
            builder.AddField(x =>
            {
                x.Name = "**Word**";
                x.IsInline = false;
                x.Value = dictionary["list"].First["word"].ToString();
            });
            builder.AddField(x =>
            {
                x.Name = "**Definition**";
                x.IsInline = false;
                x.Value = dictionary["list"].First["definition"].ToString();
            });
            builder.AddField(x =>
            {
                x.Name = "**Example**";
                x.IsInline = false;
                x.Value = dictionary["list"].First["example"].ToString();
            });
            builder.AddField(x =>
            {
                x.Name = "**URL**";
                x.IsInline = true;
                x.Value = $"[Click to View]({dictionary["list"].First["permalink"].ToString()})";
            });
            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
        [Command("8ball")]
        [Remarks("8ball <question>")]
        public async Task EightBall([Remainder] string word)
        {
            string[] answers =
            {
                ":8ball: It is certain", ":8ball: It is decidedly so", ":8ball: Without a doubt",
                ":8ball: Yes, definitely", ":8ball: Don't count on it", ":8ball: My reply is no",
                ":8ball: Outlook not so good", ":8ball: Very doubtful", ":8ball: Most likely",
                ":8ball: Reply hazy try again", ":8ball: Better not tell you now", ":8ball: Concentrate and ask again",
                ":8ball: My sources say no", ":8ball: You may rely on it", ":8ball: Cannot predict now",
                ":8ball: Concentrate and ask again", ":8ball: Ask again later", ":8ball: As I see it, yes"
            };
            await ReplyAsync(answers[rnd.Next(answers.Length)]);
        }
    }
}
