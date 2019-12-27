using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace GladosV3.Helpers
{
    public sealed class Tools
    {

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
            {
                await using Stream stream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException());
                json = await reader.ReadToEndAsync();
            }
            if (string.IsNullOrWhiteSpace(json)) return await Task.FromResult("");
            var Object = JObject.Parse(json);
            return await Task.FromResult(new WebProxy { Address = new Uri($"http://{Object["ipPort"]}") });
        }
        internal static void ReleaseMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            if (SqLite.Connection != null && SqLite.Connection.State != ConnectionState.Closed)
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

                msg += $"{(!string.IsNullOrEmpty(msg) ? "\n" : "")}{chunk}";
            }

            messages.Add(msg);
            return messages.ToArray();
        }

        public static bool WriteToReadOnlyValue(Type type, object obj, string element, object value)
        {
            foreach (PropertyInfo info in type.GetProperties())
            {
                if (info.Name != element) continue;
                if (!info.CanWrite) return false;
                info.SetValue(obj, value);
                return true;
            }
            return false;
        }
    }
}
