using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GladosV3.Helpers
{
    public class Tools
    {
        private static readonly IConfigurationBuilder Builder = new ConfigurationBuilder()    // Begin building the configuration file
            .SetBasePath(AppContext.BaseDirectory)  // Specify the location of the config
            .AddJsonFile("_configuration.json");

        public static void WriteColorLine(ConsoleColor color, string message)
        {
            var fcolor = Console.ForegroundColor;
            var bcolor = Console.BackgroundColor;
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.DarkGray;    
            Console.Out.WriteLine(message);
            Console.ForegroundColor = fcolor;
            Console.BackgroundColor = bcolor;
        }

        public static void WriteColor(ConsoleColor color, string message)
        {
            var fcolor = Console.ForegroundColor;
            var bcolor = Console.BackgroundColor;
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Out.Write(message);
            Console.ForegroundColor = fcolor;
            Console.BackgroundColor = bcolor;
        }
        public static void RestartApp()
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = "dotnet",
                    Arguments = $"{Assembly.GetEntryAssembly().Location}",
                    UseShellExecute = false,
                    WorkingDirectory = AppContext.BaseDirectory
                }
            };
            proc.Start();
            Environment.Exit(1);
        }
        public static async Task<dynamic> GetProxyAsync()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://gimmeproxy.com/api/getProxy?anonymityLevel=1&user-agent=true&protocol=http&country=GB,CZ,DE,SK,FR&minSpeed=1024"));
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            string json;
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                json = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(json)) return await Task.FromResult("");
            var Object = JObject.Parse(json);
            return await Task.FromResult(new WebProxy { Address = new Uri($"http://{Object["ipPort"]}") });
        }
        public static async Task<dynamic> GetConfigAsync(int type = 0) // 
        {
            if(type == 0)
            return await Task.FromResult(Builder.Build()); // default reading
                return await Task.FromResult(JObject.Parse(File
                    .ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json")).GetAwaiter()
                    .GetResult())); // alternative reading
        }
        internal static void ReleaseMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            if(SqLite.Connection != null && SqLite.Connection.State != ConnectionState.Closed)
                SqLite.Connection.ReleaseMemory();
        }
        public static string[] SplitMessage(string message, int len) // discord.js :D
        {
            if (message.Length <= len) return new[] { message };
            var splitText = message.Split('\n');
            if (splitText.Length == 1) throw new Exception("SPLIT_MAX_LEN");
            List<string> messages = new List<string>();
            var msg = "";
            foreach (var chunk in splitText)
            {
                if (($"{msg}\n{chunk}").Length > len)
                {
                    messages.Add(msg);
                    msg = "";
                }

                msg += $"{(msg != "" ? "\n" : "")}{chunk}";
            }

            messages.Add(msg);
            return messages.ToArray();

        }
    }
}
