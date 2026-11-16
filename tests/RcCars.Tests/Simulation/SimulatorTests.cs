using Microsoft.Extensions.Logging;
using RcCars.Cars;
using RcCars.Commands;
using RcCars.Domain;
using RcCars.Simulation;

namespace RcCars.Tests.Simulation;

public class SimulatorTests
{
    private readonly Simulator _simulator = new();
    private readonly CommandParser _parser = new();

    private SimulationResult Run(int width, int length, int x, int y, Heading heading, string commands, out RcCar car)
    {
        car = new MonsterTruck(new CarPose(new Position(x, y), heading));
        return _simulator.Run(new Room(width, length), car, _parser.Parse(commands));
    }

    [Fact]
    public void Run_AssignmentSample_HitsNorthWallOnThirdCommand()
    {
        var result = Run(8, 6, 2, 3, Heading.North, "FFFRFFLBB", out var car);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Collision);
        Assert.Equal(3, result.Collision.CommandNumber);
        Assert.Equal('F', result.Collision.Command.Code);
        Assert.Equal(new Position(2, 6), result.Collision.AttemptedPosition);
        Assert.Equal(2, result.ExecutedCommands);
        Assert.Equal(new CarPose(new Position(2, 5), Heading.North), result.FinalPose);
        Assert.Equal(result.FinalPose, car.Pose);
    }

    [Fact]
    public void Run_SampleCommandsFromLowerStart_Succeeds()
    {
        var result = Run(8, 6, 2, 1, Heading.North, "FFFRFFLBB", out var car);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Collision);
        Assert.Equal(9, result.ExecutedCommands);
        Assert.Equal(new CarPose(new Position(4, 2), Heading.North), result.FinalPose);
        Assert.Equal(result.FinalPose, car.Pose);
    }

    [Theory]
    [InlineData(0, 3, Heading.North, "F", 0, 4)]
    [InlineData(0, 0, Heading.South, "F", 0, -1)]
    [InlineData(0, 0, Heading.West, "F", -1, 0)]
    [InlineData(3, 0, Heading.East, "F", 4, 0)]
    [InlineData(0, 0, Heading.North, "B", 0, -1)]
    [InlineData(0, 0, Heading.East, "B", -1, 0)]
    public void Run_MovingOutsideRoom_ReportsCollision(
        int startX, int startY, Heading heading, string command, int expectedX, int expectedY)
    {
        var result = Run(4, 4, startX, startY, heading, command, out _);

        Assert.False(result.IsSuccess);
        Assert.Equal(new Position(expectedX, expectedY), result.Collision!.AttemptedPosition);
    }

    [Fact]
    public void Run_TurningInCorner_DoesNotHitWall()
    {
        var result = Run(1, 1, 0, 0, Heading.North, "LLLLRRRR", out _);

        Assert.True(result.IsSuccess);
        Assert.Equal(new CarPose(new Position(0, 0), Heading.North), result.FinalPose);
    }

    [Fact]
    public void Run_DrivingAlongAllWalls_Succeeds()
    {
        var result = Run(4, 4, 0, 0, Heading.North, "FFFRFFFRFFFRFFFR", out _);

        Assert.True(result.IsSuccess);
        Assert.Equal(new CarPose(new Position(0, 0), Heading.North), result.FinalPose);
    }

    [Fact]
    public void Run_StopsAtFirstCollision_RemainingCommandsNotExecuted()
    {
        var result = Run(2, 2, 0, 0, Heading.South, "FLLFF", out var car);

        Assert.Equal(1, result.Collision!.CommandNumber);
        Assert.Equal(0, result.ExecutedCommands);
        Assert.Equal(new CarPose(new Position(0, 0), Heading.South), car.Pose);
    }

    [Fact]
    public void Run_RecordsStartPoseAndExecutedSteps()
    {
        var result = Run(3, 3, 0, 0, Heading.North, "FRFB", out _);

        Assert.Equal(new CarPose(new Position(0, 0), Heading.North), result.StartPose);
        Assert.Equal(4, result.TotalCommands);
        Assert.Equal(
            new[]
            {
                new CarPose(new Position(0, 1), Heading.North),
                new CarPose(new Position(0, 1), Heading.East),
                new CarPose(new Position(1, 1), Heading.East),
                new CarPose(new Position(0, 1), Heading.East),
            },
            result.Steps.Select(step => step.Pose));
        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Steps.Select(step => step.CommandNumber));
    }

    [Fact]
    public void Run_Collision_StepsContainOnlyExecutedCommands()
    {
        var result = Run(2, 2, 0, 0, Heading.North, "FFRF", out _);

        Assert.Single(result.Steps);
        Assert.Equal(4, result.TotalCommands);
    }

    [Fact]
    public void Run_NoCommands_SucceedsAtStartPose()
    {
        var result = Run(3, 3, 1, 1, Heading.East, "", out _);

        Assert.True(result.IsSuccess);
        Assert.Equal(new CarPose(new Position(1, 1), Heading.East), result.FinalPose);
    }

    [Fact]
    public void Run_CarStartsOutsideRoom_Throws()
    {
        var car = new MonsterTruck(new CarPose(new Position(5, 5), Heading.North));

        Assert.Throws<ArgumentException>(() => _simulator.Run(new Room(2, 2), car, []));
    }

    [Fact]
    public void Run_Success_LogsStartStepsAndResult()
    {
        var logger = new TestLogger<Simulator>();
        var car = new MonsterTruck(new CarPose(new Position(0, 0), Heading.North));

        new Simulator(logger).Run(new Room(3, 3), car, _parser.Parse("FR"));

        Assert.Collection(
            logger.Entries,
            entry => Assert.Equal(
                "Simulation started: Monster Truck in a 3 x 3 m room at [0, 0] heading N with 2 command(s) 'FR'", entry.Message),
            entry => Assert.Equal("Command #1 'F' (move forward): [0, 0] heading N -> [0, 1] heading N", entry.Message),
            entry => Assert.Equal("Command #2 'R' (turn right): [0, 1] heading N -> [0, 1] heading E", entry.Message),
            entry => Assert.Equal("Simulation successful: the car ended at [0, 1] heading E after 2 command(s)", entry.Message));
        Assert.Equal(2, logger.AtLevel(LogLevel.Trace).Count());
    }

    [Fact]
    public void Run_WallHit_LogsCollisionDetails()
    {
        var logger = new TestLogger<Simulator>();
        var car = new MonsterTruck(new CarPose(new Position(2, 3), Heading.North));

        new Simulator(logger).Run(new Room(8, 6), car, _parser.Parse("FFFRFFLBB"));

        var last = logger.Entries[^1];
        Assert.Equal(LogLevel.Information, last.Level);
        Assert.Equal(
            "Simulation unsuccessful: command #3 'F' (move forward) would have moved the car from [2, 5] to [2, 6], " +
            "outside the 8 x 6 m room. 2 of 9 command(s) executed, last valid pose [2, 5] heading N",
            last.Message);
    }
}
