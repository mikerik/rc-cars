using RcCars.Domain;

namespace RcCars.Tests.Domain;

public class PositionAndRoomTests
{
    [Theory]
    [InlineData(Heading.North, 2, 4)]
    [InlineData(Heading.East, 3, 3)]
    [InlineData(Heading.South, 2, 2)]
    [InlineData(Heading.West, 1, 3)]
    public void Translate_MovesOneStepInHeadingDirection(Heading heading, int expectedX, int expectedY)
    {
        var result = new Position(2, 3).Translate(heading, 1);

        Assert.Equal(new Position(expectedX, expectedY), result);
    }

    [Fact]
    public void Translate_NegativeDistance_MovesBackwards()
    {
        Assert.Equal(new Position(2, 2), new Position(2, 3).Translate(Heading.North, -1));
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(3, 3, true)]
    [InlineData(0, 3, true)]
    [InlineData(3, 0, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(4, 0, false)]
    [InlineData(0, 4, false)]
    public void Contains_FourByFourRoom_MatchesAssignmentPicture(int x, int y, bool expected)
    {
        var room = new Room(4, 4);

        Assert.Equal(expected, room.Contains(new Position(x, y)));
    }

    [Fact]
    public void Contains_UsesWidthForXAndLengthForY()
    {
        var room = new Room(8, 6);

        Assert.True(room.Contains(new Position(7, 5)));
        Assert.False(room.Contains(new Position(5, 7)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-3, 5)]
    public void Constructor_NonPositiveSize_Throws(int width, int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Room(width, length));
    }
}
