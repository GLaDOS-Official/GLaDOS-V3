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
            if (!string.IsNullOrWhiteSpace(BatchFilePath) && TimerValue > 1)  {
                _timer = new Timer() { Enabled = true, Interval = TimerValue };
                _timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs args) => { BuildNow().GetAwaiter().GetResult(); });
            }
            else
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", "Failed to load!");
        }

        public static System.Threading.Tasks.Task LoadCIChannel()
        {
            textChannel = client.GetChannel(config["nomad"]["CIChannel"].Value<ulong>()) as SocketTextChannel;
            client.Ready -= LoadCIChannel;
            return Task.CompletedTask;
        }
        public Task BuildNow()
        {
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
                    if (!string.IsNullOrWhiteSpace(config["nomad"]["logFile"].Value<string>())) {
                        var file = File.CreateText(config["nomad"]["logFile"].Value<string>());
                        file.WriteAsync(text).GetAwaiter().GetResult();
                        file.Flush();
                        file.Close();
                    }
                    if (!string.IsNullOrWhiteSpace(text)) {
                        var array = text.Split(Environment.NewLine);
                        build = array[array.Length - 2];
                    }
                    sw.BaseStream.Flush();
                    sw.BaseStream.Close();
                    }
                process.WaitForExit();
                IncrementVersion(build);
            }
            catch(Exception ex)
            {
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", $"Exception happened during build! {ex.Message},{ex.StackTrace.ToString()}");
                textChannel.SendMessageAsync($"Exception happened during build! Details should be inside the console.").GetAwaiter().GetResult();
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
            FileVersionInfo fversion = FileVersionInfo.GetVersionInfo(output);
            List<int> array = config["nomad"]["nextVersion"].Value<string>().Split('.').ToList().ConvertAll(int.Parse);
            int bPart = array[3];
            int pPart = array[2];
            int minorPart = array[1];
            int majorPart = array[0];
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
    }
}
