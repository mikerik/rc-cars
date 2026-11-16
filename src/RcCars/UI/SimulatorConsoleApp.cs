using System.Diagnostics.CodeAnalysis;
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
public sealed class SimulatorConsoleApp
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Simulator _simulator;
    private readonly CommandParser _commandParser;

    public SimulatorConsoleApp(TextReader input, TextWriter output, Simulator simulator, CommandParser commandParser)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
    }

    /// <summary>
    /// Runs one simulation and returns a process exit code (see <see cref="ExitCodes"/>).
    /// </summary>
    public int Run()
    {
        _output.WriteLine("RC car simulator - Monster Truck");
        _output.WriteLine();

        var result = RunSimulation();
        if (result is null)
        {
            _output.WriteLine("No more input available. The simulation was aborted.");
            return ExitCodes.InputAborted;
        }

        return result.IsSuccess ? ExitCodes.Success : ExitCodes.HitWall;
    }

    /// <summary>Asks for input, runs one simulation and prints the result. Returns <c>null</c> if the input stream ends.</summary>
    private SimulationResult? RunSimulation()
    {
        if (!TryPrompt(
                "Enter the room size as '<width> <length>' in whole meters (e.g. 8 6):",
                InputParser.ParseRoom,
                out var room))
        {
            return null;
        }

        if (!TryPrompt(
                "Enter the start position and heading as '<x> <y> <N|E|S|W>' (e.g. 2 3 N):",
                line => InputParser.ParseStartPose(line, room),
                out var startPose))
        {
            return null;
        }

        if (!TryPrompt(
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

    /// <summary>
    /// Prompts until the parser accepts the input. Returns <c>false</c> if the input stream ends.
    /// </summary>
    private bool TryPrompt<T>(string prompt, Func<string, T> parse, [MaybeNullWhen(false)] out T value)
    {
        while (true)
        {
            _output.WriteLine(prompt);
            _output.Write("> ");

            var line = _input.ReadLine();
            if (line is null)
            {
                _output.WriteLine();
                value = default;
                return false;
            }

            try
            {
                value = parse(line);
                return true;
            }
            catch (InputFormatException ex)
            {
                _output.WriteLine($"Invalid input: {ex.Message}");
                _output.WriteLine();
            }
        }
    }
}
