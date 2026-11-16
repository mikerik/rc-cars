using RcCars.Domain;
using RcCars.Simulation;

namespace RcCars.UI;

/// <summary>Builds the text shown to the user after a simulation.</summary>
public static class ResultFormatter
{
    public static string Format(SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var pose = result.FinalPose;
        if (result.Collision is not { } collision)
        {
            return "Simulation successful!" + Environment.NewLine +
                   $"End position: {pose.Position.X} {pose.Position.Y} {pose.Heading.ToLetter()}";
        }

        var room = result.Room;
        return "Simulation unsuccessful: the car drove into the wall." + Environment.NewLine +
               $"Command #{collision.CommandNumber} '{collision.Command.Code}' ({collision.Command.Description}) " +
               $"would have moved the car from {pose.Position} to {collision.AttemptedPosition}, " +
               $"which is outside the room (X 0-{room.Width - 1}, Y 0-{room.Length - 1})." + Environment.NewLine +
               $"Last valid position: {pose.Position.X} {pose.Position.Y} {pose.Heading.ToLetter()} " +
               $"after {result.ExecutedCommands} command(s).";
    }
}
