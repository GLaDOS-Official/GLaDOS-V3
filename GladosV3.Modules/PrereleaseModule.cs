#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Helpers;

namespace GladosV3.Modules
{
    public class PrereleaseModule : ModuleBase<SocketCommandContext>
    {
        [Command("expander")]
        [Summary("expander <shortenered link>")]
        [Remarks("Get's destination http adress")]
        [Helpers.RequireOwner]
        public async Task ResolveIp([Remainder] string shortUrl)
        {
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
            var message = await dm.SendMessageAsync("Please wait...");
            string longUrl = default(string);
            //List<string> proxies = new List<string>(){ "http://185.82.212.95:8080", "http://185.93.3.123:8080", "http://138.201.215.79:1080","http://213.136.77.246:80", "http://213.136.89.121:80", "http://217.89.68.178:80" };
            using (var handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;
                if (handler.SupportsProxy)
                {
                    /*Random rnd = new Random();
                    var random = rnd.Next(0, proxies.Count - 1);*/
                    handler.UseProxy = true;
                    /*WebProxy proxy = new WebProxy
                    {
                        Address = new Uri(proxies[random])
                    };*/
                    var proxie = await Tools.GetProxy();
                    if (proxie.GetType() == typeof(WebProxy))
                        handler.Proxy = proxie; // no ip reveal
                }
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 5.0; SM-G920A) AppleWebKit (KHTML, like Gecko) Chrome Mobile Safari (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)"); // we are GoogleBot
                    var response = await client.GetAsync(shortUrl, HttpCompletionOption.ResponseHeadersRead);
                    longUrl = response.Headers.Contains("Location") ? $"This link leads to {response.Headers.Location.ToString()}" : "This link doesn't lead anywhere!";
                }
            }
            await message.ModifyAsync(properties => properties.Content = $"Expander has returned! {longUrl}");
        }
    }
}
#endif