using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Out.WriteLine(message);
            Console.ForegroundColor = fcolor;
            Console.BackgroundColor = bcolor;
        }

        public static void WriteColor(ConsoleColor color, string message)
        {
            var fcolor = Console.ForegroundColor;
            var bcolor = Console.BackgroundColor;
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.Gray;
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
        public static async Task<dynamic> GetConfig(int type = 0) // 
        {
            if(type == 0)
            return await Task.FromResult(Builder.Build()); // default reading
            else if (type == 1)
                return await Task.FromResult(JObject.Parse(File
                    .ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "_configuration.json")).GetAwaiter()
                    .GetResult())); // alternative reading
            return Task.CompletedTask; // this will never get called
        }
        internal static void ReleaseMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            PInvokes.EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }
    }
}
