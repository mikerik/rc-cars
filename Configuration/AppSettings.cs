namespace RcCars.Configuration;

/// <summary>
/// Root of the application settings, bound from appsettings.json,
/// appsettings.{Environment}.json, RCCARS_ environment variables and command line arguments.
/// The default values below mirror appsettings.json and are used when a setting is omitted.
/// </summary>
public sealed class AppSettings
{
    public const string CommandsSection = "Commands";
    public const string LoggingSection = "Logging";

    public CommandSettings Commands { get; set; } = new();

    public LoggingSettings Logging { get; set; } = new();

    /// <summary>Returns a description of every invalid setting; empty when all settings are valid.</summary>
    public IReadOnlyList<string> Validate() =>
    [
        .. Commands is null ? [$"{CommandsSection} is missing."] : Commands.Validate(CommandsSection),
        .. Logging is null ? [$"{LoggingSection} is missing."] : Logging.Validate(LoggingSection),
    ];
}
