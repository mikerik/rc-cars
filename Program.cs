using Microsoft.Extensions.Logging;
using RcCars.Commands;
using RcCars.Configuration;
using RcCars.Logging;
using RcCars.Simulation;
using RcCars.UI;

// Until the configuration has been loaded, log with the default settings so configuration errors are logged too.
var loggerFactory = LoggingSetup.CreateLoggerFactory(new LoggingSettings(), AppContext.BaseDirectory);
var logger = loggerFactory.CreateLogger(ProgramLog.Category);
try
{
    var settings = AppSettingsLoader.Load(args);

    var configuredLoggerFactory = LoggingSetup.CreateLoggerFactory(settings.Logging, AppContext.BaseDirectory);
    loggerFactory.Dispose();
    loggerFactory = configuredLoggerFactory;
    logger = loggerFactory.CreateLogger(ProgramLog.Category);

    ProgramLog.Starting(logger, AppSettingsLoader.EnvironmentName, typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
    ProgramLog.ConfiguredCommands(logger, DescribeCommands(settings.Commands));

    var app = new SimulatorConsoleApp(
        Console.In,
        Console.Out,
        new Simulator(loggerFactory.CreateLogger<Simulator>()),
        new CommandParser(settings.Commands),
        loggerFactory.CreateLogger<SimulatorConsoleApp>());
    var exitCode = app.Run();

    ProgramLog.Exiting(logger, exitCode);
    return exitCode;
}
catch (InvalidConfigurationException ex)
{
    ProgramLog.InvalidConfiguration(logger, AppSettingsLoader.EnvironmentName, ex.Message, ex);
    Console.Error.WriteLine($"The configuration is invalid ({AppSettingsLoader.EnvironmentName} environment):");
    Console.Error.WriteLine(ex.Message);
    ProgramLog.Exiting(logger, ExitCodes.InvalidConfiguration);
    return ExitCodes.InvalidConfiguration;
}
catch (Exception ex)
{
    ProgramLog.UnexpectedError(logger, ex);
    Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
    ProgramLog.Exiting(logger, ExitCodes.UnexpectedError);
    return ExitCodes.UnexpectedError;
}
finally
{
    loggerFactory.Dispose();
}

static string DescribeCommands(CommandSettings commands) =>
    $"{Describe(commands.Forward)} ({commands.Forward.DistanceMeters} m), " +
    $"{Describe(commands.Back)} ({commands.Back.DistanceMeters} m), " +
    $"{Describe(commands.TurnLeft)}, {Describe(commands.TurnRight)}";

static string Describe(DriveCommandSettings command) => $"{command.Code} = {command.Description}";
