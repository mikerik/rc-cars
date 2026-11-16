using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RcCars.Configuration;

/// <summary>
/// Diagnostic logging settings. Log entries are always written to the debugger output
/// (e.g. the Visual Studio Output window) and optionally to a log file.
/// </summary>
public sealed class LoggingSettings
{
    /// <summary>The <see cref="LogLevel"/> key that applies to all categories without their own entry.</summary>
    public const string DefaultCategory = "Default";

    /// <summary>
    /// Minimum level per log category (namespace or class name prefix, e.g. "RcCars.Simulation").
    /// The "Default" entry applies to all other categories.
    /// </summary>
    public Dictionary<string, MsLogLevel> LogLevel { get; set; } =
        new(StringComparer.OrdinalIgnoreCase) { [DefaultCategory] = MsLogLevel.Information };

    public FileLoggingSettings File { get; set; } = new();

    internal IEnumerable<string> Validate(string section)
    {
        if (LogLevel is null)
        {
            yield return $"{section}:{nameof(LogLevel)} is missing.";
        }
        else
        {
            foreach (var (category, level) in LogLevel)
            {
                if (!Enum.IsDefined(level))
                {
                    yield return $"{section}:{nameof(LogLevel)}:{category} must be one of " +
                                 $"{string.Join(", ", Enum.GetNames<MsLogLevel>())} (was {(int)level}).";
                }
            }
        }

        if (File is null)
        {
            yield return $"{section}:{nameof(File)} is missing.";
        }
        else if (File.Enabled && string.IsNullOrWhiteSpace(File.Path))
        {
            yield return $"{section}:{nameof(File)}:{nameof(FileLoggingSettings.Path)} must not be empty when file logging is enabled.";
        }
    }
}

/// <summary>Settings for the optional log file.</summary>
public sealed class FileLoggingSettings
{
    public bool Enabled { get; set; }

    /// <summary>The log file. Relative paths are relative to the application folder. New entries are appended.</summary>
    public string Path { get; set; } = "logs/rc-cars.log";
}
