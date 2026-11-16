namespace RcCars.Configuration;

/// <summary>The letters, descriptions and distances of the drive commands.</summary>
public sealed class CommandSettings
{
    public MoveCommandSettings Forward { get; set; } = new() { Code = 'F', Description = "move forward", DistanceMeters = 1 };

    public MoveCommandSettings Back { get; set; } = new() { Code = 'B', Description = "move back", DistanceMeters = 1 };

    public DriveCommandSettings TurnLeft { get; set; } = new() { Code = 'L', Description = "turn left" };

    public DriveCommandSettings TurnRight { get; set; } = new() { Code = 'R', Description = "turn right" };

    internal IEnumerable<string> Validate(string section)
    {
        var commands = new (string Name, DriveCommandSettings? Settings)[]
        {
            (nameof(Forward), Forward),
            (nameof(Back), Back),
            (nameof(TurnLeft), TurnLeft),
            (nameof(TurnRight), TurnRight),
        };

        foreach (var (name, settings) in commands)
        {
            if (settings is null)
            {
                yield return $"{section}:{name} is missing.";
                continue;
            }

            foreach (var error in settings.Validate($"{section}:{name}"))
            {
                yield return error;
            }
        }

        var duplicateCodes = commands
            .Where(command => command.Settings is not null)
            .GroupBy(command => char.ToUpperInvariant(command.Settings!.Code))
            .Where(group => group.Count() > 1);
        foreach (var group in duplicateCodes)
        {
            yield return $"{section}: the commands {string.Join(", ", group.Select(command => command.Name))} " +
                         $"all use the code '{group.Key}'. Each command needs its own code.";
        }
    }
}

/// <summary>Settings shared by all drive commands.</summary>
public class DriveCommandSettings
{
    /// <summary>The letter used for the command in the input (case insensitive).</summary>
    public char Code { get; set; }

    /// <summary>Short description used in messages, e.g. "move forward".</summary>
    public string Description { get; set; } = string.Empty;

    internal virtual IEnumerable<string> Validate(string section)
    {
        if (Code == '\0' || char.IsWhiteSpace(Code) || char.IsControl(Code))
        {
            yield return $"{section}:{nameof(Code)} must be a single visible character.";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            yield return $"{section}:{nameof(Description)} must not be empty.";
        }
    }
}

/// <summary>Settings for commands that move the car.</summary>
public sealed class MoveCommandSettings : DriveCommandSettings
{
    /// <summary>How far the car moves, in whole meters.</summary>
    public int DistanceMeters { get; set; }

    internal override IEnumerable<string> Validate(string section)
    {
        foreach (var error in base.Validate(section))
        {
            yield return error;
        }

        if (DistanceMeters < 1)
        {
            yield return $"{section}:{nameof(DistanceMeters)} must be at least 1 (was {DistanceMeters}).";
        }
    }
}
