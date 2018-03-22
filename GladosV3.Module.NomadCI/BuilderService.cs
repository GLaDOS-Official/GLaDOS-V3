using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using GladosV3.Helpers;
using Discord.WebSocket;
using GladosV3.Services;
using Discord;
using System.IO.Compression;

namespace GladosV3.Module.NomadCI
{
    public class BuilderService
    {
        internal static JObject config;
        internal static bool IsBuilding = false;
        internal static double TimerValue = 0;
        internal static Timer _timer;
        internal string BatchFilePath;
        internal static Discord.WebSocket.DiscordSocketClient client;
        public static Discord.WebSocket.SocketTextChannel textChannel;
        public BuilderService()
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..\\Binaries\\StampVer.exe"))) { LoggingService.Log(LogSeverity.Error, "NomadCI", "StampVer.exe not found!"); return; }
            BatchFilePath = config["nomad"]["batPath"].Value<string>();
            if (!File.Exists(BatchFilePath))
            {
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", $"Batch file not found : {BatchFilePath}");
                BatchFilePath = null;
            }
            TimerValue = config["nomad"]["time"].Value<Double>();
            if (!string.IsNullOrWhiteSpace(BatchFilePath) && TimerValue > 1)
            {
                _timer = new Timer() { Enabled = true, Interval = TimerValue };
                _timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs args) => { BuildNow().GetAwaiter().GetResult(); });
            }
            else
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", "Failed to load!");
        }

        public static Task LoadCIChannel()
        {
            textChannel = client.GetChannel(config["nomad"]["CIChannel"].Value<ulong>()) as SocketTextChannel;
            client.Ready -= LoadCIChannel;
            return Task.CompletedTask;
        }
        public Task BuildNow()
        {
            if (textChannel == null) return Task.CompletedTask;
            if (string.IsNullOrWhiteSpace(BatchFilePath) || TimerValue < 1) { textChannel.SendMessageAsync("Failed to build! Check the config file!").GetAwaiter().GetResult(); return Task.CompletedTask; }
            if (IsBuilding) { textChannel.SendMessageAsync("Sorry pal, it's currently building!").GetAwaiter().GetResult(); return Task.CompletedTask; }
            IsBuilding = true;
            textChannel.SendMessageAsync("Build started! Build command has been disabled!").GetAwaiter().GetResult();
            _timer.Stop();
            try
            {
                string build = "";
                Process process = Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{BatchFilePath}\"")
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true
                });
                using (StreamReader sw = process.StandardOutput)
                {
                    string text = sw.ReadToEndAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrWhiteSpace(config["nomad"]["logFile"].Value<string>()))
                    {
                        var file = File.CreateText(config["nomad"]["logFile"].Value<string>());
                        file.WriteAsync(text).GetAwaiter().GetResult();
                        file.Flush();
                        file.Close();
                    }
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var array = text.Split(Environment.NewLine).Distinct();
                        foreach (var line in array)
                        {
                            if (line.StartsWith("OUTDIR: "))
                            { build = line.Remove(0, 8); break; }
                        }
                    }
                    sw.BaseStream.Flush();
                    sw.BaseStream.Close();
                }
                process.WaitForExit();
                Dictionary<string, NomadJsonObject> objects = new Dictionary<string, NomadJsonObject>();
                CreateObjects(build, objects);
                Compress(new DirectoryInfo(build), objects);
                BuildJson(build, objects);
                //IncrementVersion(build);
            }
            catch (Exception ex)
            {
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", $"Exception happened during build!{Environment.NewLine}   {ex.Message}{Environment.NewLine}   Type: {ex.GetType()}{Environment.NewLine}{ex.StackTrace.ToString()}");
                textChannel.SendMessageAsync($"Exception happened during build! Details should be inside the console.").GetAwaiter().GetResult();
                _timer.Interval = TimerValue;
                _timer.Start();
                IsBuilding = false;
                return Task.CompletedTask;
            }
            _timer.Interval = TimerValue;
            _timer.Start();
            IsBuilding = false;
            string TryingToBeFunnyHereLol = string.IsNullOrWhiteSpace(config["nomad"]["logFile"].Value<string>()) ? "oh wait......" : null;
            textChannel.SendMessageAsync($"Done! Should be compiled! Build command has been enabled. Also, log is available... you know where :^) {TryingToBeFunnyHereLol}").GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
        internal void IncrementVersion(string output)
        {
            if (!File.Exists(output))
                throw new IOException("Output file not found! Please check the logs.");
            FileVersionInfo fversion = FileVersionInfo.GetVersionInfo(output);
            List<int> array = config["nomad"]["nextVersion"].Value<string>().Split('.').ToList().ConvertAll(int.Parse);
            int bPart = array[3]; // build number
            int pPart = array[2]; // revision number
            int minorPart = array[1]; // minor number
            int majorPart = array[0]; // major number
            bPart++;
            if (bPart > config["nomad"]["bPart"].Value<Int32>() + 100)
            { pPart++; config["nomad"]["bPart"] = bPart; }
            else if (pPart > 250)
            { minorPart++; pPart = 0; }
            else if (minorPart > 100)
            { majorPart++; minorPart = 0; }
            string version = $"{majorPart}.{minorPart}.{pPart}.{bPart}";
            config["nomad"]["nextVersion"] = version;
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "_configuration.json"), config.ToString());
            Process process = Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..\\Binaries\\StampVer.exe"), $"-k -f\"{version}\" -p\"{version}\" {output}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }
        internal void BuildJson(string output, Dictionary<string, NomadJsonObject> objects)
        {
            JArray array = new JArray();
            foreach (var pattern in new string[] { "*.exe", "*.dll" })
                foreach (var file in Directory.GetFiles(output, pattern)) // , "*.exe|*.dll"
                {
                    objects.TryGetValue(Path.GetFileName(file), out NomadJsonObject nomadJsonObject);
                    JToken value = JValue.FromObject(nomadJsonObject);
                    array.Add(value);
                }
            if (array.Count <= 0)
                throw new FileNotFoundException("No exe and dll files found in the output folder!");
            File.WriteAllText(Path.Combine(output, "GLaDOS.CI.json"), array.ToString());
        }
        internal void Compress(DirectoryInfo directorySelected, Dictionary<string, NomadJsonObject> objects)
        {
            using (var zip = ZipFile.Open(Path.Combine(directorySelected.FullName, "release.zip"), ZipArchiveMode.Update))
                foreach (FileInfo fileToCompress in directorySelected.GetFiles())
                    if (fileToCompress.Name != "release.zip")
                        if (fileToCompress.Extension == ".dll" || fileToCompress.Extension == ".exe")
                            if (fileToCompress.Length >= 10485760)
                                if (objects.TryGetValue(fileToCompress.Name, out NomadJsonObject nomadObject))
                                { zip.CreateEntryFromFile(fileToCompress.FullName, fileToCompress.Name, CompressionLevel.Optimal); nomadObject.Zipped = true; }

        }
        internal void CreateObjects(string output, Dictionary<string, NomadJsonObject> objects)
        {
            foreach (var pattern in new string[] { "*.exe", "*.dll" })
                foreach (var file in Directory.GetFiles(output, pattern)) // , "*.exe|*.dll"
                {
                    byte[] hash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        hash = md5.ComputeHash(File.ReadAllBytes(file));
                    }
                    var nomadObject = new NomadJsonObject(Path.GetFileName(file), hash, new FileInfo(file).Length);
                    objects.Add(Path.GetFileName(file), nomadObject);
                }
        }
    }
    internal class NomadJsonObject
    {
        public string Name;
        public byte[] MD5Hash;
        public long Size;
        public bool Zipped = false;

        public NomadJsonObject(string name, byte[] hash, long size)
        {
            Name = name;
            MD5Hash = hash;
            Size = size;
        }
    }
}
