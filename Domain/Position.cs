namespace RcCars.Domain;

/// <summary>
/// A grid coordinate in whole meters. X grows towards east, Y grows towards north.
/// [0, 0] is the most south-west position a car fits in.
/// </summary>
public readonly record struct Position(int X, int Y)
{
    /// <summary>Returns the position <paramref name="distance"/> meters away in the given direction.</summary>
    public Position Translate(Heading heading, int distance) => heading switch
    {
        Heading.North => this with { Y = Y + distance },
        Heading.East => this with { X = X + distance },
        Heading.South => this with { Y = Y - distance },
        Heading.West => this with { X = X - distance },
        _ => throw new ArgumentOutOfRangeException(nameof(heading), heading, "Unknown heading."),
    };

    public override string ToString() => $"[{X}, {Y}]";
}
