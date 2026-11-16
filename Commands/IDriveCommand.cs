using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>
/// A single instruction sent to the car. Commands are pure: they compute the
/// resulting pose without touching the car, so the simulator can validate the
/// move before it is applied.
/// </summary>
public interface IDriveCommand
{
    /// <summary>The letter used for this command in the input, e.g. 'F'.</summary>
    char Code { get; }

    /// <summary>Short description used in messages, e.g. "move forward".</summary>
    string Description { get; }

    CarPose Apply(CarPose pose);
}
