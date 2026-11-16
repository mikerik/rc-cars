using RcCars.Domain;

namespace RcCars.Tests.Domain;

public class HeadingTests
{
    [Theory]
    [InlineData(Heading.North, Heading.West)]
    [InlineData(Heading.West, Heading.South)]
    [InlineData(Heading.South, Heading.East)]
    [InlineData(Heading.East, Heading.North)]
    public void TurnLeft_RotatesCounterClockwise(Heading start, Heading expected)
    {
        Assert.Equal(expected, start.TurnLeft());
    }

    [Theory]
    [InlineData(Heading.North, Heading.East)]
    [InlineData(Heading.East, Heading.South)]
    [InlineData(Heading.South, Heading.West)]
    [InlineData(Heading.West, Heading.North)]
    public void TurnRight_RotatesClockwise(Heading start, Heading expected)
    {
        Assert.Equal(expected, start.TurnRight());
    }

    [Theory]
    [InlineData('N', Heading.North)]
    [InlineData('e', Heading.East)]
    [InlineData('S', Heading.South)]
    [InlineData('w', Heading.West)]
    public void TryFromLetter_ParsesValidLettersCaseInsensitive(char letter, Heading expected)
    {
        Assert.True(HeadingExtensions.TryFromLetter(letter, out var heading));
        Assert.Equal(expected, heading);
    }

    [Theory]
    [InlineData('X')]
    [InlineData('1')]
    [InlineData(' ')]
    public void TryFromLetter_RejectsInvalidLetters(char letter)
    {
        Assert.False(HeadingExtensions.TryFromLetter(letter, out _));
    }

    [Theory]
    [InlineData(Heading.North, 'N')]
    [InlineData(Heading.East, 'E')]
    [InlineData(Heading.South, 'S')]
    [InlineData(Heading.West, 'W')]
    public void ToLetter_ReturnsCompassLetter(Heading heading, char expected)
    {
        Assert.Equal(expected, heading.ToLetter());
    }

    [Fact]
    public void ToLetter_UndefinedHeading_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Heading)42).ToLetter());
    }
}
