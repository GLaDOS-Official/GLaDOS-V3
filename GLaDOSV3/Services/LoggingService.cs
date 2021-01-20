using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    public class LoggingService
    {
        public static Task Log(LogSeverity severity, string source, string message, Exception exception = null) => OnLogAsync(new LogMessage(severity, source, message, exception));
        private static string LogDirectory => Path.Combine(AppContext.BaseDirectory, "logs");
        private static string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");
        private static readonly ObservableCollection<string> Logs = new ObservableCollection<string>();
        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            if (discord == null || commands == null) return;
            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }
        public static void Begin()
        {
            if (!Directory.Exists(LogDirectory))     // Create the log directory if it doesn't exist
                Directory.CreateDirectory(LogDirectory);
        }

        public static Task OnLogAsync(LogMessage msg)
        {
            if (!File.Exists(LogFile))               // Create today's log file if it doesn't exist
                File.Create(LogFile).Dispose();
            string logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            Logs.Add(logText);
            if (Logs.Count >= 60)
            {
                File.AppendAllText(LogFile, string.Join(Environment.NewLine, Logs));  // Write the log text to a file
                Logs.Clear();
            }
            switch (msg.Severity) // Write the log text to the console
            {
                case LogSeverity.Critical:
                    Tools.WriteColorLine(ConsoleColor.DarkRed, logText);
                    break;
                case LogSeverity.Error:
                    Tools.WriteColorLine(ConsoleColor.Red, logText);
                    break;
                case LogSeverity.Warning:
                    Tools.WriteColorLine(ConsoleColor.DarkYellow, logText);
                    break;
                case LogSeverity.Debug:
                    Tools.WriteColorLine(ConsoleColor.Yellow, logText);
                    break;
                case LogSeverity.Info:
                    Console.Out.WriteLine(logText);
                    break;
                case LogSeverity.Verbose:
                    Tools.WriteColorLine(ConsoleColor.Gray, logText);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
