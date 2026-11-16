using RcCars.Commands;
using RcCars.Domain;

namespace RcCars.Simulation;

/// <summary>Details about the command that made the car hit a wall.</summary>
/// <param name="CommandNumber">1-based index of the failing command.</param>
/// <param name="Command">The command that could not be executed.</param>
/// <param name="AttemptedPosition">The position the car tried to move to.</param>
public sealed record WallCollision(int CommandNumber, IDriveCommand Command, Position AttemptedPosition);

/// <summary>A successfully executed command and the car's pose after it.</summary>
/// <param name="CommandNumber">1-based index of the command.</param>
public sealed record SimulationStep(int CommandNumber, IDriveCommand Command, CarPose Pose);

/// <summary>
/// Outcome of a simulation run.
/// </summary>
public sealed class SimulationResult
{
    private SimulationResult(
        Room room, CarPose startPose, IReadOnlyList<SimulationStep> steps, int totalCommands, WallCollision? collision)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(steps);

        Room = room;
        StartPose = startPose;
        Steps = steps;
        TotalCommands = totalCommands;
        Collision = collision;
    }

    public Room Room { get; }

    public CarPose StartPose { get; }

    /// <summary>The commands that were executed successfully, in order.</summary>
    public IReadOnlyList<SimulationStep> Steps { get; }

    /// <summary>Number of commands the simulation was asked to execute.</summary>
    public int TotalCommands { get; }

    /// <summary>
    /// The car's pose at the end of the run. When the car hit a wall this is
    /// the last valid pose before the failing command.
    /// </summary>
    public CarPose FinalPose => Steps.Count > 0 ? Steps[^1].Pose : StartPose;

    /// <summary>Number of commands that were executed successfully.</summary>
    public int ExecutedCommands => Steps.Count;

    /// <summary>Set when the car hit a wall, otherwise <c>null</c>.</summary>
    public WallCollision? Collision { get; }

    public bool IsSuccess => Collision is null;

    public static SimulationResult Success(Room room, CarPose startPose, IReadOnlyList<SimulationStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        return new(room, startPose, steps, steps.Count, collision: null);
    }

    public static SimulationResult HitWall(
        Room room, CarPose startPose, IReadOnlyList<SimulationStep> steps, int totalCommands, WallCollision collision)
    {
        ArgumentNullException.ThrowIfNull(collision);
        return new(room, startPose, steps, totalCommands, collision);
    }
}
