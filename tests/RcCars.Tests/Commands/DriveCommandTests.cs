using RcCars.Commands;
using RcCars.Domain;

namespace RcCars.Tests.Commands;

public class DriveCommandTests
{
    private static readonly CarPose Start = new(new Position(2, 3), Heading.East);

    [Fact]
    public void Forward_MovesOneMeterAlongHeading()
    {
        Assert.Equal(new CarPose(new Position(3, 3), Heading.East), new ForwardCommand().Apply(Start));
    }

    [Fact]
    public void Back_MovesOneMeterAgainstHeadingAndKeepsHeading()
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
    public void Commands_UseTheLettersFromTheAssignment()
    {
        IDriveCommand[] commands = [new ForwardCommand(), new BackCommand(), new TurnLeftCommand(), new TurnRightCommand()];

        Assert.Equal(['F', 'B', 'L', 'R'], commands.Select(command => command.Code));
    }
}
