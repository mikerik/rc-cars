using RcCars.Configuration;
using RcCars.Domain;

namespace RcCars.Commands;

/// <summary>Base class for commands whose code and description come from configuration.</summary>
public abstract class ConfiguredDriveCommand : IDriveCommand
{
    protected ConfiguredDriveCommand(DriveCommandSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Description, nameof(settings));

        Code = settings.Code;
        Description = settings.Description;
    }

    public char Code { get; }

    public string Description { get; }

    public abstract CarPose Apply(CarPose pose);
}

/// <summary>Moves the car <see cref="DistanceMeters"/> in the direction it is facing.</summary>
public sealed class ForwardCommand : ConfiguredDriveCommand
{
    public ForwardCommand()
        : this(new CommandSettings().Forward)
    {
    }

    public ForwardCommand(MoveCommandSettings settings)
        : base(settings)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.DistanceMeters, nameof(settings));
        DistanceMeters = settings.DistanceMeters;
    }

    public int DistanceMeters { get; }

    public override CarPose Apply(CarPose pose) =>
        pose with { Position = pose.Position.Translate(pose.Heading, DistanceMeters) };
}

/// <summary>Moves the car <see cref="DistanceMeters"/> backwards, keeping its heading.</summary>
public sealed class BackCommand : ConfiguredDriveCommand
{
    public BackCommand()
        : this(new CommandSettings().Back)
    {
    }

    public BackCommand(MoveCommandSettings settings)
        : base(settings)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.DistanceMeters, nameof(settings));
        DistanceMeters = settings.DistanceMeters;
    }

    public int DistanceMeters { get; }

    public override CarPose Apply(CarPose pose) =>
        pose with { Position = pose.Position.Translate(pose.Heading, -DistanceMeters) };
}

/// <summary>Rotates the car 90° counter clockwise on the spot.</summary>
public sealed class TurnLeftCommand : ConfiguredDriveCommand
{
    public TurnLeftCommand()
        : this(new CommandSettings().TurnLeft)
    {
    }

    public TurnLeftCommand(DriveCommandSettings settings)
        : base(settings)
    {
    }

    public override CarPose Apply(CarPose pose) => pose with { Heading = pose.Heading.TurnLeft() };
}

/// <summary>Rotates the car 90° clockwise on the spot.</summary>
public sealed class TurnRightCommand : ConfiguredDriveCommand
{
    public TurnRightCommand()
        : this(new CommandSettings().TurnRight)
    {
    }

    public TurnRightCommand(DriveCommandSettings settings)
        : base(settings)
    {
    }

    public override CarPose Apply(CarPose pose) => pose with { Heading = pose.Heading.TurnRight() };
}
