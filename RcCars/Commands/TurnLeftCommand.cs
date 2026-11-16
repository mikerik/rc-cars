using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>Rotates the car 90° counter clockwise on the spot.</summary>
public sealed class TurnLeftCommand : IDriveCommand
{
    public char Code => 'L';

    public string Description => "turn left";

    public CarPose Apply(CarPose pose) => pose with { Heading = pose.Heading.TurnLeft() };
}
