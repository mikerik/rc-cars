using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RcCars.Cars;
using RcCars.Commands;
using RcCars.Domain;

namespace RcCars.Simulation;

/// <summary>
/// Drives a car around a room and detects if it hits a wall.
/// </summary>
public sealed partial class Simulator
{
    private readonly ILogger _logger;

    public Simulator(ILogger<Simulator>? logger = null)
    {
        _logger = logger ?? NullLogger<Simulator>.Instance;
    }

    /// <summary>
    /// Executes the commands in order. The simulation stops at the first command
    /// that would take the car outside the room.
    /// </summary>
    /// <exception cref="ArgumentException">The car does not start inside the room.</exception>
    public SimulationResult Run(Room room, RcCar car, IReadOnlyList<IDriveCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(car);
        ArgumentNullException.ThrowIfNull(commands);

        if (!room.Contains(car.Pose.Position))
        {
            throw new ArgumentException($"The car's start position {car.Pose.Position} is outside the room.", nameof(car));
        }

        var startPose = car.Pose;
        var steps = new List<SimulationStep>(commands.Count);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            LogStarted(_logger, car.Model, room, startPose, commands.Count, string.Concat(commands.Select(command => command.Code)));
        }

        for (var i = 0; i < commands.Count; i++)
        {
            var command = commands[i];
            var nextPose = command.Apply(car.Pose);

            if (!room.Contains(nextPose.Position))
            {
                var collision = new WallCollision(i + 1, command, nextPose.Position);
                var result = SimulationResult.HitWall(room, startPose, steps, commands.Count, collision);
                LogWallHit(
                    _logger, collision.CommandNumber, command.Code, command.Description, car.Pose.Position,
                    collision.AttemptedPosition, room, result.ExecutedCommands, result.TotalCommands, result.FinalPose);
                return result;
            }

            LogStep(_logger, i + 1, command.Code, command.Description, car.Pose, nextPose);
            car.MoveTo(nextPose);
            steps.Add(new SimulationStep(i + 1, command, nextPose));
        }

        LogSucceeded(_logger, car.Pose, steps.Count);
        return SimulationResult.Success(room, startPose, steps);
    }

    [LoggerMessage(EventId = 200, Level = LogLevel.Information,
        Message = "Simulation started: {CarModel} in a {Room} room at {StartPose} with {CommandCount} command(s) '{Commands}'")]
    private static partial void LogStarted(
        ILogger logger, string carModel, Room room, CarPose startPose, int commandCount, string commands);

    [LoggerMessage(EventId = 201, Level = LogLevel.Trace,
        Message = "Command #{CommandNumber} '{CommandCode}' ({CommandDescription}): {FromPose} -> {ToPose}")]
    private static partial void LogStep(
        ILogger logger, int commandNumber, char commandCode, string commandDescription, CarPose fromPose, CarPose toPose);

    [LoggerMessage(EventId = 202, Level = LogLevel.Information,
        Message = "Simulation successful: the car ended at {FinalPose} after {ExecutedCommands} command(s)")]
    private static partial void LogSucceeded(ILogger logger, CarPose finalPose, int executedCommands);

    [LoggerMessage(EventId = 203, Level = LogLevel.Information,
        Message = "Simulation unsuccessful: command #{CommandNumber} '{CommandCode}' ({CommandDescription}) would have moved " +
                  "the car from {FromPosition} to {AttemptedPosition}, outside the {Room} room. " +
                  "{ExecutedCommands} of {TotalCommands} command(s) executed, last valid pose {FinalPose}")]
    private static partial void LogWallHit(
        ILogger logger, int commandNumber, char commandCode, string commandDescription, Position fromPosition,
        Position attemptedPosition, Room room, int executedCommands, int totalCommands, CarPose finalPose);
}
