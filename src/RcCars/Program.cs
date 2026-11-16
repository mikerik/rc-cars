using RcCars.Commands;
using RcCars.Simulation;
using RcCars.UI;

try
{
    var app = new SimulatorConsoleApp(Console.In, Console.Out, new Simulator(), CommandParser.Default);
    return app.Run();
}
catch (Exception ex)
{
    // Expected problems (invalid input, wall hits) are handled inside the app; anything reaching this point is a bug.
    Console.Error.WriteLine($"An unexpected error occurred: {ex}");
    return ExitCodes.UnexpectedError;
}
