namespace RcCars.Domain;

/// <summary>
/// The compass direction a car is facing.
/// </summary>
public enum Heading
{
    North,
    East,
    South,
    West,
}

public static class HeadingExtensions
{
    /// <summary>Rotates the heading 90° counter clockwise.</summary>
    public static Heading TurnLeft(this Heading heading) => heading switch
    {
        Heading.North => Heading.West,
        Heading.West => Heading.South,
        Heading.South => Heading.East,
        Heading.East => Heading.North,
        _ => throw UnknownHeading(heading),
    };

    /// <summary>Rotates the heading 90° clockwise.</summary>
    public static Heading TurnRight(this Heading heading) => heading switch
    {
        Heading.North => Heading.East,
        Heading.East => Heading.South,
        Heading.South => Heading.West,
        Heading.West => Heading.North,
        _ => throw UnknownHeading(heading),
    };

    /// <summary>Returns the single letter representation (N, E, S or W).</summary>
    public static char ToLetter(this Heading heading) => heading switch
    {
        Heading.North => 'N',
        Heading.East => 'E',
        Heading.South => 'S',
        Heading.West => 'W',
        _ => throw UnknownHeading(heading),
    };

    /// <summary>Parses a heading letter (N, E, S or W, case insensitive).</summary>
    public static bool TryFromLetter(char letter, out Heading heading)
    {
        switch (char.ToUpperInvariant(letter))
        {
            case 'N': heading = Heading.North; return true;
            case 'E': heading = Heading.East; return true;
            case 'S': heading = Heading.South; return true;
            case 'W': heading = Heading.West; return true;
            default: heading = default; return false;
        }
    }

    private static ArgumentOutOfRangeException UnknownHeading(Heading heading) =>
        new(nameof(heading), heading, "Unknown heading.");
}
