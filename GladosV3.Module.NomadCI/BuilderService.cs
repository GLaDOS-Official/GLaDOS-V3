using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using GladosV3.Services;
using Newtonsoft.Json.Linq;

namespace GladosV3.Module.NomadCI
{
    public class BuilderService
    {
        #region Variables
        public static BuilderService Service;
        internal static JObject config;
        internal static bool IsBuilding;
        internal static double TimerValue;
        internal static Timer _timer;
        internal string BatchFilePath;
        internal static DiscordSocketClient client;
        public static SocketTextChannel textChannel;
        internal static DateTime nextBuildTime;
        #region IncremenentVersion pinvoke stuff
        [DllImport("VersionIncrementer.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int VersionIncrement(string filename, string fileversion);
        #endregion
        #endregion

        public BuilderService()
        {
            if(!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PInvoke\\VersionIncrementer.dll"))) { LoggingService.Log(LogSeverity.Error, "NomadCI", "VersionIncrementer not found in the PInvoke directory!"); return; }
            BatchFilePath = config["nomad"]["batPath"].Value<string>();
            if (!File.Exists(BatchFilePath))
            {
                LoggingService.Log(LogSeverity.Error, "NomadCI", $"Batch file not found : {BatchFilePath}");
                BatchFilePath = null;
            }
            TimerValue = config["nomad"]["time"].Value<Double>();
            nextBuildTime = DateTime.Now.AddMilliseconds(TimerValue);
            if (!string.IsNullOrWhiteSpace(BatchFilePath) && TimerValue > 1)
            {
                _timer = new Timer { Enabled = true, Interval = TimerValue };
                _timer.Elapsed += (sender, args) => { BuildNow().GetAwaiter().GetResult(); };
            }
            else
                LoggingService.Log(LogSeverity.Error, "NomadCI", "Failed to load!");
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
                            if (!line.StartsWith("OUTDIR: ")) continue;
                            build = line.Remove(0, 8); break;
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
                IncrementVersionTask(build);
            }
            catch (Exception ex)
            {
                LoggingService.Log(LogSeverity.Error, "NomadCI", $"Exception happened during build!{Environment.NewLine}   {ex.Message}{Environment.NewLine}   Type: {ex.GetType()}{Environment.NewLine}{ex.StackTrace}");
                textChannel.SendMessageAsync($"Exception happened during build! Details should be inside the console.").GetAwaiter().GetResult();
                _timer.Interval = TimerValue;
                _timer.Start();
                IsBuilding = false;
                return Task.CompletedTask;
            }
            _timer.Interval = TimerValue;
            nextBuildTime = DateTime.Now.AddMilliseconds(TimerValue);
            _timer.Start();
            IsBuilding = false;
            string TryingToBeFunnyHereLol = string.IsNullOrWhiteSpace(config["nomad"]["logFile"].Value<string>()) ? "oh wait......" : null;
            textChannel.SendMessageAsync($"Done! Should be compiled! Build command has been. Also, log is available... you know where :^) {TryingToBeFunnyHereLol}").GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
        internal void IncrementVersionTask(string output)
        {
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
            foreach (var pattern in new[] { "*.exe", "*.dll" })
                foreach (var file in Directory.GetFiles(output, pattern)) // , "*.exe|*.dll"
                {
                    var response = VersionIncrement(file, version);
                    if (response != 0)
                        throw new Exception($"Something failed during versionincrement! Response code: {response}");
                }
        }
        internal void BuildJson(string output, Dictionary<string, NomadJsonObject> objects)
        {
            JArray array = new JArray();
            foreach (var pattern in new[] { "*.exe", "*.dll" })
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
            foreach (string pattern in new[] { "*.exe", "*.dll" })
                foreach (var file in Directory.GetFiles(output, pattern)) // , "*.exe|*.dll"
                {
                    byte[] hash;
                    using (var md5 = MD5.Create())
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
        public bool Zipped;

        public NomadJsonObject(string name, byte[] hash, long size)
        {
            Name = name;
            MD5Hash = hash;
            Size = size;
        }
    }
}
