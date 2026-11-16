using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>Rotates the car 90° clockwise on the spot.</summary>
public sealed class TurnRightCommand : IDriveCommand
{
    public char Code => 'R';

    public string Description => "turn right";

    public CarPose Apply(CarPose pose) => pose with { Heading = pose.Heading.TurnRight() };
}
