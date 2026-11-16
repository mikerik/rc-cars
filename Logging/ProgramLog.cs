using Microsoft.Extensions.Logging;

namespace RcCars.Logging;

/// <summary>Log messages written by the composition root (Program.cs).</summary>
internal static partial class ProgramLog
{
    public const string Category = "RcCars.Program";

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "RC car simulator starting ({EnvironmentName} environment, version {Version})")]
    public static partial void Starting(ILogger logger, string environmentName, string version);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Configured commands: {Commands}")]
    public static partial void ConfiguredCommands(ILogger logger, string commands);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "RC car simulator exiting with code {ExitCode}")]
    public static partial void Exiting(ILogger logger, int exitCode);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "The configuration is invalid ({EnvironmentName} environment): {Errors}")]
    public static partial void InvalidConfiguration(ILogger logger, string environmentName, string errors, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "An unexpected error occurred")]
    public static partial void UnexpectedError(ILogger logger, Exception exception);
}
