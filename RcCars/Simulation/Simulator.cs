using RcCars.Cars;
using RcCars.Commands;
using RcCars.Domain;

namespace RcCars.Simulation;

/// <summary>
/// Drives a car around a room and detects if it hits a wall.
/// </summary>
public sealed class Simulator
{
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

        for (var i = 0; i < commands.Count; i++)
        {
            var command = commands[i];
            var nextPose = command.Apply(car.Pose);

            if (!room.Contains(nextPose.Position))
            {
                var collision = new WallCollision(i + 1, command, nextPose.Position);
                return SimulationResult.HitWall(room, startPose, steps, commands.Count, collision);
            }

            car.MoveTo(nextPose);
            steps.Add(new SimulationStep(i + 1, command, nextPose));
        }

        return SimulationResult.Success(room, startPose, steps);
    }
}
