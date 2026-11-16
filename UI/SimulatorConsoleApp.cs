using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RcCars.Cars;
using RcCars.Commands;
using RcCars.Input;
using RcCars.Simulation;

namespace RcCars.UI;

/// <summary>
/// Interactive console front end. Reads input from a <see cref="TextReader"/> and
/// writes to a <see cref="TextWriter"/> so it can be driven by tests or piped input.
/// Invalid input is reported and the user is asked again.
/// </summary>
public sealed partial class SimulatorConsoleApp
{
    private static readonly Func<ILogger, int, IDisposable?> SimulationScope =
        LoggerMessage.DefineScope<int>("Simulation #{SimulationNumber}");

    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Simulator _simulator;
    private readonly CommandParser _commandParser;
    private readonly ILogger _logger;

    public SimulatorConsoleApp(
        TextReader input,
        TextWriter output,
        Simulator simulator,
        CommandParser commandParser,
        ILogger<SimulatorConsoleApp>? logger = null)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
        _logger = logger ?? NullLogger<SimulatorConsoleApp>.Instance;
    }

    /// <summary>
    /// Runs simulations until the user chooses to exit and returns a process exit code
    /// (see <see cref="ExitCodes"/>) based on the last simulation.
    /// </summary>
    public int Run()
    {
        _output.WriteLine("RC car simulator - Monster Truck");

        var exitCode = ExitCodes.Success;
        var simulationNumber = 1;
        for (; ; simulationNumber++)
        {
            using var scope = SimulationScope(_logger, simulationNumber);
            LogSimulationStarting(_logger, simulationNumber);

            _output.WriteLine();
            _output.WriteLine($"--- Simulation #{simulationNumber} ---");

            var result = RunSimulation();
            if (result is null)
            {
                LogSimulationAborted(_logger, simulationNumber);
                _output.WriteLine("No more input available. The simulation was aborted.");
                return ExitCodes.InputAborted;
            }

            exitCode = result.IsSuccess ? ExitCodes.Success : ExitCodes.HitWall;

            if (!AskToRunAgain())
            {
                break;
            }
        }

        LogFinished(_logger, simulationNumber);
        _output.WriteLine();
        _output.WriteLine("Exiting the simulator. Goodbye!");
        return exitCode;
    }

    /// <summary>Asks for input, runs one simulation and prints the result. Returns <c>null</c> if the input stream ends.</summary>
    private SimulationResult? RunSimulation()
    {
        if (!TryPrompt(
                "room size",
                "Enter the room size as '<width> <length>' in whole meters (e.g. 8 6):",
                InputParser.ParseRoom,
                out var room))
        {
            return null;
        }

        if (!TryPrompt(
                "start position",
                "Enter the start position and heading as '<x> <y> <N|E|S|W>' (e.g. 2 3 N):",
                line => InputParser.ParseStartPose(line, room),
                out var startPose))
        {
            return null;
        }

        if (!TryPrompt(
                "commands",
                $"Enter the commands to execute ({_commandParser.AvailableCodes}), e.g. FFRFLB:",
                _commandParser.Parse,
                out var commands))
        {
            return null;
        }

        var car = new MonsterTruck(startPose);
        var result = _simulator.Run(room, car, commands);

        _output.WriteLine();
        _output.WriteLine(ResultFormatter.Format(result));
        _output.WriteLine();

        return result;
    }

    /// <summary>Returns <c>true</c> if the user wants a new simulation. End of input counts as "no".</summary>
    private bool AskToRunAgain() =>
        TryPrompt(
            "run again answer",
            "Do you want to run a new simulation? (Y)es to start over, (N)o to exit:",
            InputParser.ParseYesNo,
            out var runAgain)
        && runAgain;

    /// <summary>
    /// Prompts until the parser accepts the input. Returns <c>false</c> if the input stream ends.
    /// </summary>
    /// <param name="inputName">Name of the requested value, used in log messages.</param>
    private bool TryPrompt<T>(string inputName, string prompt, Func<string, T> parse, [MaybeNullWhen(false)] out T value)
    {
        while (true)
        {
            _output.WriteLine(prompt);
            _output.Write("> ");

            var line = _input.ReadLine();
            if (line is null)
            {
                LogEndOfInput(_logger, inputName);
                _output.WriteLine();
                value = default;
                return false;
            }

            try
            {
                value = parse(line);
                LogInputAccepted(_logger, inputName, line);
                return true;
            }
            catch (InputFormatException ex)
            {
                LogInvalidInput(_logger, inputName, line, ex.Message);
                _output.WriteLine($"Invalid input: {ex.Message}");
                _output.WriteLine();
            }
        }
    }

    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Simulation #{SimulationNumber} starting")]
    private static partial void LogSimulationStarting(ILogger logger, int simulationNumber);

    [LoggerMessage(EventId = 101, Level = LogLevel.Debug, Message = "Accepted {InputName} input '{Input}'")]
    private static partial void LogInputAccepted(ILogger logger, string inputName, string input);

    [LoggerMessage(EventId = 102, Level = LogLevel.Warning, Message = "Invalid {InputName} input '{Input}': {Reason}")]
    private static partial void LogInvalidInput(ILogger logger, string inputName, string input, string reason);

    [LoggerMessage(EventId = 103, Level = LogLevel.Debug, Message = "End of input while waiting for the {InputName}")]
    private static partial void LogEndOfInput(ILogger logger, string inputName);

    [LoggerMessage(EventId = 104, Level = LogLevel.Warning, Message = "Input ended during simulation #{SimulationNumber}; the simulation was aborted")]
    private static partial void LogSimulationAborted(ILogger logger, int simulationNumber);

    [LoggerMessage(EventId = 105, Level = LogLevel.Information, Message = "No new simulation requested; exiting after {SimulationCount} simulation(s)")]
    private static partial void LogFinished(ILogger logger, int simulationCount);
}
