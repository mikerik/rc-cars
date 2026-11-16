using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>Moves the car one meter backwards. The car keeps its heading.</summary>
public sealed class BackCommand : IDriveCommand
{
    private const int DistanceMeters = 1;

    public char Code => 'B';

    public string Description => "move back";

    public CarPose Apply(CarPose pose) =>
        pose with { Position = pose.Position.Translate(pose.Heading, -DistanceMeters) };
}
