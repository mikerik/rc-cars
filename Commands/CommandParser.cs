using RcCars.Configuration;
using RcCars.Input;

namespace RcCars.Commands;

/// <summary>
/// Translates a command string such as "FFRBL" into drive commands.
/// </summary>
public sealed class CommandParser
{
    private readonly IReadOnlyDictionary<char, IDriveCommand> _commandsByCode;

    public CommandParser()
        : this(new CommandSettings())
    {
    }

    public CommandParser(CommandSettings settings)
        : this(CreateCommands(settings))
    {
    }

    public CommandParser(IEnumerable<IDriveCommand> availableCommands)
    {
        ArgumentNullException.ThrowIfNull(availableCommands);

        var commandsByCode = new Dictionary<char, IDriveCommand>();
        foreach (var command in availableCommands)
        {
            var code = char.ToUpperInvariant(command.Code);
            if (!commandsByCode.TryAdd(code, command))
            {
                throw new ArgumentException($"More than one command uses the code '{code}'.", nameof(availableCommands));
            }
        }

        _commandsByCode = commandsByCode;
    }

    private static IEnumerable<IDriveCommand> CreateCommands(CommandSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return
        [
            new ForwardCommand(settings.Forward),
            new BackCommand(settings.Back),
            new TurnLeftCommand(settings.TurnLeft),
            new TurnRightCommand(settings.TurnRight),
        ];
    }

    /// <summary>The valid command letters, e.g. "F, B, L, R".</summary>
    public string AvailableCodes => string.Join(", ", _commandsByCode.Keys);

    /// <summary>
    /// Parses the command letters (case insensitive). Whitespace between commands is ignored.
    /// </summary>
    /// <exception cref="InputFormatException">The text contains an unknown command letter.</exception>
    public IReadOnlyList<IDriveCommand> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var commands = new List<IDriveCommand>(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            if (!_commandsByCode.TryGetValue(char.ToUpperInvariant(character), out var command))
            {
                throw new InputFormatException(
                    $"Unknown command '{character}' at character {i + 1}. Valid commands are {AvailableCodes}.");
            }

            commands.Add(command);
        }

        return commands;
    }
}
