using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace GladosV3.Module.NomadCI
{
    public class BuilderService
    {
        internal static IConfigurationRoot config;
        internal static bool IsBuilding = false;
        internal static double TimerValue = 0;
        internal static Timer _timer;
        internal string BatchFilePath;
        public BuilderService()
        {

            BatchFilePath = config["nomad:batPath"];
            if (!File.Exists(BatchFilePath))
            {
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", $"Batch file not found : {BatchFilePath}");
                BatchFilePath = null;
            }
            if (Double.TryParse(config["nomad:time"], out double value))
                TimerValue = value;
            else
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", "Variable 'time' is not a valid double!");
            if (!string.IsNullOrWhiteSpace(BatchFilePath) && value > 1)  {
                _timer = new Timer() { Enabled = true, Interval = TimerValue };
                _timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs args) => { BuildNow(); });
            }
            else
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", "Failed to load!");
        }
        public Task<string> BuildNow()
        {
            if (string.IsNullOrWhiteSpace(BatchFilePath) || TimerValue < 1) return Task.FromResult("Failed to build! Check the config file!");
            if (IsBuilding) return Task.FromResult("Sorry pal, it's currently building!");
            IsBuilding = true;
            _timer.Stop();
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c \"{BatchFilePath}\"")
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                if (!string.IsNullOrWhiteSpace(config["nomad:logFile"]))
                    psi.RedirectStandardOutput = true;
                Process process = Process.Start(psi);
                if (psi.RedirectStandardOutput)
                {
                    using (StreamReader sw = process.StandardOutput)
                    {
                        var file = File.CreateText(config["nomad:logFile"]);
                        file.WriteAsync(sw.ReadToEndAsync().GetAwaiter().GetResult()).GetAwaiter().GetResult();
                        file.Flush();
                        file.Close();
                        sw.BaseStream.Flush();
                    }
                }
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                GladosV3.Services.LoggingService.Log(Discord.LogSeverity.Error, "NomadCI", $"Exception happened during build! {ex.Message},{ex.StackTrace.ToString()}");
            }
            //IncrementVersion();
            _timer.Interval = TimerValue;
            _timer.Start(); 
            IsBuilding = false;
            string TryingToBeFunnyHereLol = string.IsNullOrWhiteSpace(config["nomad:logFile"]) ? "oh wait......" : null;
            return Task.FromResult($"Done! Should be compiled! Log is available... you know where :^) {TryingToBeFunnyHereLol}");
        }
        internal void IncrementVersion()
        {
            Version version = 
        }
    }
}
