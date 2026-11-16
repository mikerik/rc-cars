namespace RcCars.Domain;

/// <summary>
/// Where a car is and which way it is facing.
/// </summary>
public readonly record struct CarPose(Position Position, Heading Heading)
{
    public override string ToString() => $"{Position} heading {Heading.ToLetter()}";
}
