using Microsoft.Extensions.Logging;
using RcCars.Configuration;

namespace RcCars.Logging;

/// <summary>Creates the application's <see cref="ILoggerFactory"/> from <see cref="LoggingSettings"/>.</summary>
public static partial class LoggingSetup
{
    /// <summary>
    /// Creates a logger factory that writes to the debugger output and, when enabled, to the log file.
    /// A log file that cannot be opened is reported as a warning instead of stopping the application.
    /// </summary>
    /// <param name="baseDirectory">The folder relative log file paths are resolved against.</param>
    public static ILoggerFactory CreateLoggerFactory(LoggingSettings settings, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddDebug();
            foreach (var (category, level) in settings.LogLevel)
            {
                if (string.Equals(category, LoggingSettings.DefaultCategory, StringComparison.OrdinalIgnoreCase))
                {
                    builder.SetMinimumLevel(level);
                }
                else
                {
                    builder.AddFilter(category, level);
                }
            }
        });

        var logger = factory.CreateLogger(typeof(LoggingSetup).FullName!);
        if (settings.File.Enabled)
        {
            var path = Path.GetFullPath(settings.File.Path, baseDirectory);
            try
            {
                // Providers added to the factory directly are disposed (and the file closed) with the factory.
                factory.AddProvider(new FileLoggerProvider(path));
                LogFileOpened(logger, path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
            {
                LogFileUnavailable(logger, path, ex);
            }
        }

        return factory;
    }

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Writing log entries to {LogFilePath}")]
    private static partial void LogFileOpened(ILogger logger, string logFilePath);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "File logging is disabled: the log file {LogFilePath} could not be opened")]
    private static partial void LogFileUnavailable(ILogger logger, string logFilePath, Exception exception);
}
