namespace GLaDOSV3.Services
{
    /*  public class LoggingService
      {
          private static ILogger<LoggingService> _logger;
          //public static Task Log(LogSeverity severity, string source, string message, Exception exception = null) => OnLogAsync(new LogMessage(severity, source, message, exception));
          private static string LogDirectory => Path.Combine(AppContext.BaseDirectory, "logs");
          // DiscordShardedClient and CommandService are injected automatically from the IServiceProvider
          public LoggingService(DiscordShardedClient discord, CommandService commands, ILogger<LoggingService> logger)
          {
              if (discord == null || commands == null) return;
              _logger =  logger;
              discord.Log  += OnLogAsync;
              commands.Log += OnLogAsync;
          }
          public static void Begin()
          {
              if (!Directory.Exists(LogDirectory))     // Create the log directory if it doesn't exist
                  Directory.CreateDirectory(LogDirectory);
          }
          private static LogLevel GetLogLevel(LogSeverity severity)
              => (LogLevel)Math.Abs((int)severity - 5);

          public static Task OnLogAsync(LogMessage msg)
          {
              var logLevel = (LogLevel)Math.Abs((int)msg.Severity - 5);
              if (msg.Exception == null)
                  _logger.Log(logLevel, msg.Message);
              else 
                  _logger.Log(logLevel, msg.Exception, msg.Message);

              return Task.CompletedTask;
          }
      }*/
}
