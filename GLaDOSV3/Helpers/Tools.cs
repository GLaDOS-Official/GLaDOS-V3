using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GladosV3.Helpers
{
    public class Tools
    {
        private static readonly IConfigurationBuilder builder = new ConfigurationBuilder()    // Begin building the configuration file
            .SetBasePath(AppContext.BaseDirectory)  // Specify the location of the config
            .AddJsonFile("_configuration.json");
        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        private static readonly object MessageLock = new object();
        public static void WriteColorMessage(ConsoleColor color,string message)
        {
            lock (MessageLock)
            {
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public static async Task<dynamic> GetProxy() // 
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://gimmeproxy.com/api/getProxy?anonymityLevel=1&user-agent=true&protocol=http"));
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            string json;
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                json = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(json)) return await Task.FromResult("");
            var Object = JObject.Parse(json);
            return await Task.FromResult(new WebProxy { Address = new Uri($"http://{Object["ipPort"].ToString()}") });
        }
        public static async Task<IConfigurationRoot> GetConfig() // 
        {
            return await Task.FromResult(builder.Build());
        }
    }
}
