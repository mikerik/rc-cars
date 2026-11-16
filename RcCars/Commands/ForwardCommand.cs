using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>Moves the car one meter in the direction it is facing.</summary>
public sealed class ForwardCommand : IDriveCommand
{
    private const int DistanceMeters = 1;

    public char Code => 'F';

    public string Description => "move forward";

    public CarPose Apply(CarPose pose) =>
        pose with { Position = pose.Position.Translate(pose.Heading, DistanceMeters) };
}
