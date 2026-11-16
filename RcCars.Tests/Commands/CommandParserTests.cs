using RcCars.Commands;
using RcCars.Input;

namespace RcCars.Tests.Commands;

public class CommandParserTests
{
    private readonly CommandParser _parser = CommandParser.Default;

    [Fact]
    public void Parse_ValidString_ReturnsCommandsInOrder()
    {
        var commands = _parser.Parse("FBLR");

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
        var commands = _parser.Parse(" f F\tr ");

        Assert.Equal(['F', 'F', 'R'], commands.Select(c => c.Code));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyInput_ReturnsNoCommands(string text)
    {
        Assert.Empty(_parser.Parse(text));
    }

    [Fact]
    public void Parse_UnknownCommand_ThrowsWithCharacterAndPosition()
    {
        var ex = Assert.Throws<InputFormatException>(() => _parser.Parse("FFX"));

        Assert.Contains("'X'", ex.Message);
        Assert.Contains("character 3", ex.Message);
        Assert.Contains("F, B, L, R", ex.Message);
    }

    [Fact]
    public void AvailableCodes_ListsTheAssignmentCommands()
    {
        Assert.Equal("F, B, L, R", _parser.AvailableCodes);
    }

    [Fact]
    public void Constructor_DuplicateCodes_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CommandParser([new ForwardCommand(), new ForwardCommand()]));
    }
}
