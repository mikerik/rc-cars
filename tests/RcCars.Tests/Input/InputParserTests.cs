using RcCars.Domain;
using RcCars.Input;

namespace RcCars.Tests.Input;

public class InputParserTests
{
    private static readonly Room Room = new(8, 6);

    [Theory]
    [InlineData("8 6")]
    [InlineData("  8   6  ")]
    [InlineData("8\t6")]
    public void ParseRoom_ValidInput_ReturnsRoom(string line)
    {
        var room = InputParser.ParseRoom(line);

        Assert.Equal(8, room.Width);
        Assert.Equal(6, room.Length);
    }

    [Theory]
    [InlineData("", "Expected 2 values")]
    [InlineData("8", "Expected 2 values")]
    [InlineData("8 6 4", "Expected 2 values")]
    [InlineData("8 six", "Length 'six'")]
    [InlineData("8.5 6", "Width '8.5'")]
    [InlineData("99999999999 6", "Width '99999999999'")]
    [InlineData("0 6", "at least 1 meter")]
    [InlineData("8 -1", "at least 1 meter")]
    public void ParseRoom_InvalidInput_ThrowsDescriptiveError(string line, string expectedMessagePart)
    {
        var ex = Assert.Throws<InputFormatException>(() => InputParser.ParseRoom(line));

        Assert.Contains(expectedMessagePart, ex.Message);
    }

    [Theory]
    [InlineData("2 3 N", 2, 3, Heading.North)]
    [InlineData("0 0 s", 0, 0, Heading.South)]
    [InlineData(" 7  5  W ", 7, 5, Heading.West)]
    public void ParseStartPose_ValidInput_ReturnsPose(string line, int x, int y, Heading heading)
    {
        Assert.Equal(new CarPose(new Position(x, y), heading), InputParser.ParseStartPose(line, Room));
    }

    [Theory]
    [InlineData("2 3", "Expected 3 values")]
    [InlineData("2 3 N E", "Expected 3 values")]
    [InlineData("a 3 N", "X 'a'")]
    [InlineData("2 b N", "Y 'b'")]
    [InlineData("2 3 X", "Heading 'X'")]
    [InlineData("2 3 NE", "Heading 'NE'")]
    [InlineData("8 0 N", "outside the room")]
    [InlineData("0 6 N", "outside the room")]
    [InlineData("-1 0 N", "outside the room")]
    public void ParseStartPose_InvalidInput_ThrowsDescriptiveError(string line, string expectedMessagePart)
    {
        var ex = Assert.Throws<InputFormatException>(() => InputParser.ParseStartPose(line, Room));

        Assert.Contains(expectedMessagePart, ex.Message);
    }
}
