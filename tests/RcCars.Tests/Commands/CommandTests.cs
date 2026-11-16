using RcCars.Commands;
using RcCars.Configuration;
using RcCars.Domain;
using RcCars.Input;

namespace RcCars.Tests.Commands;

public class CommandTests
{
    private static readonly CarPose Start = new(new Position(2, 3), Heading.East);

    [Fact]
    public void Forward_MovesAlongHeading()
    {
        Assert.Equal(new CarPose(new Position(3, 3), Heading.East), new ForwardCommand().Apply(Start));
    }

    [Fact]
    public void Back_MovesAgainstHeadingAndKeepsHeading()
    {
        Assert.Equal(new CarPose(new Position(1, 3), Heading.East), new BackCommand().Apply(Start));
    }

    [Fact]
    public void TurnLeft_RotatesWithoutMoving()
    {
        Assert.Equal(new CarPose(new Position(2, 3), Heading.North), new TurnLeftCommand().Apply(Start));
    }

    [Fact]
    public void TurnRight_RotatesWithoutMoving()
    {
        Assert.Equal(new CarPose(new Position(2, 3), Heading.South), new TurnRightCommand().Apply(Start));
    }

    [Fact]
    public void Parse_ValidString_ReturnsCommandsInOrder()
    {
        var commands = new CommandParser().Parse("FBLR");

        Assert.Collection(
            commands,
            c => Assert.IsType<ForwardCommand>(c),
            c => Assert.IsType<BackCommand>(c),
            c => Assert.IsType<TurnLeftCommand>(c),
            c => Assert.IsType<TurnRightCommand>(c));
    }

    [Fact]
    public void Parse_IsCaseInsensitiveAndIgnoresWhitespace()
    {
        var commands = new CommandParser().Parse(" f F\tr ");

        Assert.Equal(['F', 'F', 'R'], commands.Select(c => c.Code));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyInput_ReturnsNoCommands(string text)
    {
        Assert.Empty(new CommandParser().Parse(text));
    }

    [Fact]
    public void Parse_UnknownCommand_ThrowsWithCharacterAndPosition()
    {
        var ex = Assert.Throws<InputFormatException>(() => new CommandParser().Parse("FFX"));

        Assert.Contains("'X'", ex.Message);
        Assert.Contains("character 3", ex.Message);
    }

    [Fact]
    public void Constructor_DuplicateCodes_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CommandParser([new ForwardCommand(), new ForwardCommand()]));
    }

    [Fact]
    public void Parse_CustomSettings_UsesConfiguredCodesDescriptionsAndDistances()
    {
        var settings = new CommandSettings
        {
            Forward = new MoveCommandSettings { Code = 'U', Description = "go up", DistanceMeters = 2 },
            Back = new MoveCommandSettings { Code = 'D', Description = "go down", DistanceMeters = 3 },
            TurnLeft = new DriveCommandSettings { Code = 'Q', Description = "rotate left" },
            TurnRight = new DriveCommandSettings { Code = 'E', Description = "rotate right" },
        };
        var parser = new CommandParser(settings);

        var commands = parser.Parse("uqde");

        Assert.Equal("U, D, Q, E", parser.AvailableCodes);
        Assert.Equal(["go up", "rotate left", "go down", "rotate right"], commands.Select(c => c.Description));
        Assert.Equal(new CarPose(new Position(4, 3), Heading.East), commands[0].Apply(Start));
        Assert.Equal(new CarPose(new Position(-1, 3), Heading.East), commands[2].Apply(Start));
        Assert.Throws<InputFormatException>(() => parser.Parse("F"));
    }

    [Fact]
    public void MoveCommand_NonPositiveDistance_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ForwardCommand(new MoveCommandSettings { Code = 'F', Description = "move forward", DistanceMeters = 0 }));
    }
}
