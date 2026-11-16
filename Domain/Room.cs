namespace RcCars.Domain;

/// <summary>
/// A rectangular room measured in whole meters.
/// A car (1 m in diameter) fits at every position where 0 &lt;= X &lt; Width and 0 &lt;= Y &lt; Length.
/// </summary>
public sealed class Room
{
    public Room(int width, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        Width = width;
        Length = length;
    }

    /// <summary>Size in meters along the west-east (X) axis.</summary>
    public int Width { get; }

    /// <summary>Size in meters along the south-north (Y) axis.</summary>
    public int Length { get; }

    public bool Contains(Position position) =>
        position.X >= 0 && position.X < Width &&
        position.Y >= 0 && position.Y < Length;

    public override string ToString() => $"{Width} x {Length} m";
}
