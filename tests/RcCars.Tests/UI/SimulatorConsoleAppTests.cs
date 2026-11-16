using Microsoft.Extensions.Logging;
using RcCars.Commands;
using RcCars.Simulation;
using RcCars.UI;

namespace RcCars.Tests.UI;

public class SimulatorConsoleAppTests
{
    private static (int ExitCode, string Output) RunApp(params string[] inputLines) =>
        RunApp(new TestLogger<SimulatorConsoleApp>(), inputLines);

    private static (int ExitCode, string Output) RunApp(TestLogger<SimulatorConsoleApp> logger, params string[] inputLines)
    {
        using var input = new StringReader(string.Join(Environment.NewLine, inputLines));
        using var output = new StringWriter();
        var app = new SimulatorConsoleApp(input, output, new Simulator(), new CommandParser(), logger);

        var exitCode = app.Run();

        return (exitCode, output.ToString());
    }

    [Fact]
    public void Run_SuccessfulSimulation_PrintsEndPositionAndReturnsSuccess()
    {
        var (exitCode, output) = RunApp("8 6", "2 1 N", "FFFRFFLBB");

        Assert.Equal(ExitCodes.Success, exitCode);
        Assert.Contains("Simulation successful!", output);
        Assert.Contains("End position: 4 2 N", output);
    }

    [Fact]
    public void Run_AssignmentSample_PrintsCollisionAndReturnsHitWall()
    {
        var (exitCode, output) = RunApp("8 6", "2 3 N", "FFFRFFLBB");

        Assert.Equal(ExitCodes.HitWall, exitCode);
        Assert.Contains("Simulation unsuccessful", output);
        Assert.Contains("Command #3 'F'", output);
        Assert.Contains("from [2, 5] to [2, 6]", output);
        Assert.Contains("Last valid position: 2 5 N", output);
    }

    [Fact]
    public void Run_InvalidInput_ReportsErrorAndAsksAgain()
    {
        var (exitCode, output) = RunApp("8", "8 6", "9 9 N", "2 1 N", "FFZ", "FFF");

        Assert.Equal(ExitCodes.Success, exitCode);
        Assert.Contains("Invalid input: Expected 2 values", output);
        Assert.Contains("Invalid input: Start position [9, 9] is outside the room", output);
        Assert.Contains("Invalid input: Unknown command 'Z'", output);
        Assert.Contains("End position: 2 4 N", output);
    }

    [Theory]
    [InlineData(new object[] { new string[0] })]
    [InlineData(new object[] { new[] { "8 6" } })]
    [InlineData(new object[] { new[] { "8 6", "2 1 N" } })]
    [InlineData(new object[] { new[] { "8 6", "2 1 N", "F", "Y", "4 4" } })]
    public void Run_InputEndsDuringSimulation_AbortsGracefully(string[] lines)
    {
        var (exitCode, output) = RunApp(lines);

        Assert.Equal(ExitCodes.InputAborted, exitCode);
        Assert.Contains("simulation was aborted", output);
    }

    [Fact]
    public void Run_AfterSimulation_AsksToRunAgain()
    {
        var (_, output) = RunApp("8 6", "2 1 N", "F", "N");

        Assert.Contains("Do you want to run a new simulation?", output);
        Assert.Contains("Goodbye", output);
        Assert.DoesNotContain("--- Simulation #2 ---", output);
    }

    [Fact]
    public void Run_AnswerYes_StartsNewSimulationAndReturnsLastResult()
    {
        var (exitCode, output) = RunApp(
            "8 6", "2 3 N", "FFFRFFLBB", "y",
            "8 6", "2 1 N", "FFFRFFLBB", "no");

        Assert.Equal(ExitCodes.Success, exitCode);
        Assert.Contains("--- Simulation #2 ---", output);
        Assert.Contains("Simulation unsuccessful", output);
        Assert.Contains("End position: 4 2 N", output);
        Assert.Contains("Goodbye", output);
    }

    [Fact]
    public void Run_LastSimulationHitWall_ReturnsHitWall()
    {
        var (exitCode, _) = RunApp("3 3", "0 0 N", "F", "Y", "3 3", "0 0 S", "F", "N");

        Assert.Equal(ExitCodes.HitWall, exitCode);
    }

    [Fact]
    public void Run_InvalidRunAgainAnswer_AsksAgain()
    {
        var (exitCode, output) = RunApp("8 6", "2 1 N", "F", "maybe", "n");

        Assert.Equal(ExitCodes.Success, exitCode);
        Assert.Contains("Invalid input: 'maybe' is not a valid answer", output);
        Assert.Contains("Goodbye", output);
    }

    [Fact]
    public void Run_InputEndsAtRunAgainPrompt_ExitsWithLastResult()
    {
        var (exitCode, output) = RunApp("8 6", "2 3 N", "FFF");

        Assert.Equal(ExitCodes.HitWall, exitCode);
        Assert.Contains("Goodbye", output);
        Assert.DoesNotContain("aborted", output);
    }

    [Fact]
    public void Run_InvalidInput_LogsWarningPerInvalidValue()
    {
        var logger = new TestLogger<SimulatorConsoleApp>();

        RunApp(logger, "8", "8 6", "2 1 N", "FFZ", "FFF", "N");

        Assert.Collection(
            logger.AtLevel(LogLevel.Warning),
            entry => Assert.StartsWith("Invalid room size input '8': Expected 2 values", entry.Message),
            entry => Assert.StartsWith("Invalid commands input 'FFZ': Unknown command 'Z'", entry.Message));
    }

    [Fact]
    public void Run_Simulations_LogStartAndValidInput()
    {
        var logger = new TestLogger<SimulatorConsoleApp>();

        RunApp(logger, "8 6", "2 1 N", "F", "Y", "3 3", "0 0 N", "F", "N");

        var messages = logger.Entries.Select(entry => entry.Message).ToList();
        Assert.Contains("Simulation #1 starting", messages);
        Assert.Contains("Simulation #2 starting", messages);
        Assert.Contains("Accepted start position input '2 1 N'", messages);
        Assert.Contains("No new simulation requested; exiting after 2 simulation(s)", messages);
        Assert.Empty(logger.AtLevel(LogLevel.Warning));
    }

    [Fact]
    public void Run_InputEndsDuringSimulation_LogsWarning()
    {
        var logger = new TestLogger<SimulatorConsoleApp>();

        RunApp(logger, "8 6");

        var warning = Assert.Single(logger.AtLevel(LogLevel.Warning));
        Assert.Equal("Input ended during simulation #1; the simulation was aborted", warning.Message);
    }
}
