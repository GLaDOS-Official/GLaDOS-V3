using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;

namespace GladosV3.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        public static Task Log(LogSeverity severity, string source, string message, Exception exception = null) =>
            OnLogAsync(new LogMessage(severity, source, message, exception));
        private static string _logDirectory => Path.Combine(AppContext.BaseDirectory, "logs");
        private static string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");
        private static ObservableCollection<string> logs = new ObservableCollection<string>();
        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, CommandService commands, bool init = true)
        {
            _discord = discord;
            _commands = commands;
            if (!init) return;
            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }
        public static void Begin()
        {
            if (!Directory.Exists(_logDirectory))     // Create the log directory if it doesn't exist
                Directory.CreateDirectory(_logDirectory);
        }

        private static Task OnLogAsync(LogMessage msg)
        {
            if (!File.Exists(_logFile))               // Create today's log file if it doesn't exist
                File.Create(_logFile).Dispose();
            string logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            logs.Add(logText);
            if (logs.Count >= 60) {
                File.AppendAllText(_logFile, string.Join(Environment.NewLine, logs));  // Write the log text to a file
                logs.Clear();
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
