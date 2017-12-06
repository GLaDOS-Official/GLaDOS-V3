using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace GladosV3.Helpers
{
    public class RestartApp
    {
        public static void Restart()
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
    }
}
