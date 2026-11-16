using System.Globalization;
using RcCars.Domain;

namespace RcCars.Input;

/// <summary>
/// Parses and validates the room and start position lines entered by the user.
/// </summary>
public static class InputParser
{
    /// <summary>Parses "&lt;width&gt; &lt;length&gt;", e.g. "8 6".</summary>
    /// <exception cref="InputFormatException">The input is malformed or describes an invalid room.</exception>
    public static Room ParseRoom(string line)
    {
        var parts = Tokenize(line, expectedCount: 2, expectedFormat: "<width> <length>, e.g. '8 6'");

        var width = ParseInteger(parts[0], "Width");
        var length = ParseInteger(parts[1], "Length");

        if (width <= 0 || length <= 0)
        {
            throw new InputFormatException("The room width and length must both be at least 1 meter.");
        }

        return new Room(width, length);
    }

    /// <summary>Parses "&lt;x&gt; &lt;y&gt; &lt;heading&gt;", e.g. "2 3 N", and checks that the car fits in the room.</summary>
    /// <exception cref="InputFormatException">The input is malformed or the position is outside the room.</exception>
    public static CarPose ParseStartPose(string line, Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        var parts = Tokenize(line, expectedCount: 3, expectedFormat: "<x> <y> <heading>, e.g. '2 3 N'");

        var x = ParseInteger(parts[0], "X");
        var y = ParseInteger(parts[1], "Y");

        if (parts[2].Length != 1 || !HeadingExtensions.TryFromLetter(parts[2][0], out var heading))
        {
            throw new InputFormatException($"Heading '{parts[2]}' is not valid. Use one of N, E, S or W.");
        }

        var position = new Position(x, y);
        if (!room.Contains(position))
        {
            throw new InputFormatException(
                $"Start position {position} is outside the room. " +
                $"X must be 0-{room.Width - 1} and Y must be 0-{room.Length - 1}.");
        }

        return new CarPose(position, heading);
    }

    /// <summary>Parses a yes/no answer: Y, YES, N or NO (case insensitive).</summary>
    /// <exception cref="InputFormatException">The answer is neither yes nor no.</exception>
    public static bool ParseYesNo(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return line.Trim().ToUpperInvariant() switch
        {
            "Y" or "YES" => true,
            "N" or "NO" => false,
            _ => throw new InputFormatException($"'{line.Trim()}' is not a valid answer. Please answer Y (yes) or N (no)."),
        };
    }

    private static string[] Tokenize(string line, int expectedCount, string expectedFormat)
    {
        ArgumentNullException.ThrowIfNull(line);

        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != expectedCount)
        {
            throw new InputFormatException(
                $"Expected {expectedCount} values separated by spaces but got {parts.Length}. Format: {expectedFormat}.");
        }

        return parts;
    }

    private static int ParseInteger(string text, string name)
    {
        if (!int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
        {
            throw new InputFormatException($"{name} '{text}' is not a valid whole number.");
        }

        return value;
    }
}
