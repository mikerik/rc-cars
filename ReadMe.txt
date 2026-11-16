RC CAR SIMULATOR - MONSTER TRUCK
================================

A console application that simulates a radio controlled Monster Truck driving
around a rectangular room and reports whether it can execute a series of
commands without hitting a wall.

Written in C# for .NET 10 (Windows or Linux).


HOW TO BUILD AND RUN
--------------------
Requirements: .NET 10 SDK (or newer).

  Visual Studio: open rc-cars.slnx, set "rc-cars" as start-up project, press F5.
                 Run the unit tests from Test Explorer.

  Command line (from the solution folder):
    dotnet run --project rc-cars.csproj
    dotnet test tests\RcCars.Tests\RcCars.Tests.csproj

For each simulation the simulator asks for three lines of input:

  1. Room size:              <width> <length>        e.g.  8 6
  2. Start position/heading: <x> <y> <N|E|S|W>       e.g.  2 3 N
  3. Commands:               any sequence of F/B/L/R e.g.  FFFRFFLBB

After the result has been printed the simulator asks whether you want to run
a new simulation. Answer Y (yes) to start over with a new room, or N (no) to
exit the simulator.

Input can also be piped, e.g. (PowerShell):
    "5 5", "1 1 N", "FFRFF", "N" | dotnet run --project rc-cars.csproj

Exit codes (useful when the simulator is scripted):
  0 = the last simulation was successful
  1 = the car drove into a wall in the last simulation
  2 = input ended in the middle of a simulation (e.g. Ctrl+Z / Ctrl+D)
  3 = unexpected internal error
  4 = the configuration is invalid (see CONFIGURATION)


CONFIGURATION
-------------
The tunable values are read at startup from configuration instead of being
hard coded. Sources are applied in this order; later ones win:

  1. appsettings.json                   base settings (all values listed)
  2. appsettings.<Environment>.json     optional per-environment overrides
  3. Environment variables RCCARS_*     e.g. RCCARS_Commands__Forward__DistanceMeters=2
                                        (use __ between section names)
  4. Command line arguments             e.g. --Commands:Forward:DistanceMeters=2

The environment is taken from DOTNET_ENVIRONMENT (default: Production).
Shipped files:
  appsettings.json              Production defaults.
  appsettings.Development.json  Development overrides (verbose logging to the
                                debugger and a log file, see LOGGING).
                                Visual Studio (F5) and "dotnet run" use the
                                Development profile from
                                Properties\launchSettings.json; use
                                "dotnet run --no-launch-profile" for Production.
To add another environment (e.g. Staging) create appsettings.Staging.json with
only the values that differ and set DOTNET_ENVIRONMENT=Staging.

Settings:
  Commands:Forward|Back:Code / Description / DistanceMeters   (F, B; 1 m)
  Commands:TurnLeft|TurnRight:Code / Description              (L, R)
  Logging:LogLevel:Default                  (Information)
  Logging:LogLevel:<category>               e.g. Logging:LogLevel:RcCars.Simulation=Trace
  Logging:File:Enabled / Path               (false; logs/rc-cars.log)

The settings are validated at startup. Unknown keys (typos), values of the
wrong type and invalid values (e.g. duplicate command codes, a distance below
1 m) are all reported together and the program exits with code 4. Values left
out of a file fall back to the defaults above.

The user facing texts (prompts and messages), the heading letters N/E/S/W
and the exit codes are intentionally not configurable: they are part of the
program's input/output contract.


LOGGING
-------
Diagnostic log entries are written separately from the console output (so they
never mix with the prompts):

  * Debugger output - always on. Visible in the Visual Studio Output window
                      (Show output from: Debug) while debugging.
  * Log file        - when Logging:File:Enabled is true. Relative paths are
                      relative to the application folder (e.g.
                      bin\Debug\net10.0\logs\rc-cars.log); entries are appended.

The Production defaults log Information and above with no log file. The
Development environment logs Debug and above, Trace for RcCars.Simulation, and
enables the log file. To turn on the log file for a single run:
    dotnet run --project rc-cars.csproj -- --Logging:File:Enabled=true --Logging:LogLevel:Default=Trace

What is logged, by level:
  Critical     unexpected errors (with stack trace)
  Error        invalid configuration
  Warning      invalid user input (and the reason), input ending in the middle
               of a simulation, a log file that cannot be opened
  Information  start/exit (environment, exit code), each simulation's start
               (room, start pose, commands) and its result (end pose, or the
               failing command, attempted position and last valid pose)
  Debug        configured commands, every accepted input value
  Trace        every executed command with the pose before and after it

File entries look like this; "=> Simulation #n" tells which simulation an
entry belongs to:
  2026-11-16 15:27:21.898 +01:00 [INF] RcCars.Simulation.Simulator[203] => Simulation #1: Simulation unsuccessful: ...

Configuration errors are logged with the default logging settings (debugger
output only), since the configured logging settings may themselves be invalid.


EXAMPLE
-------
Note that the sample from the assignment (room 8 6, start 2 3 N, commands
FFFRFFLBB) results in a collision: the room is 6 meters long so the valid Y
coordinates are 0-5, and the third "F" would move the car from [2, 5] to
[2, 6]. Output:

  Simulation unsuccessful: the car drove into the wall.
  Command #3 'F' (move forward) would have moved the car from [2, 5] to [2, 6],
  which is outside the room (X 0-7, Y 0-5).
  Last valid position: 2 5 N after 2 command(s).

A successful run, e.g. room 8 6, start 2 1 N, commands FFFRFFLBB, prints:

  Simulation successful!
  End position: 4 2 N


ASSUMPTIONS
-----------
Coordinate system and room
  * X runs west -> east, Y runs south -> north. North means increasing Y.
  * The first room value is the width (X axis), the second the length (Y axis).
  * The car is 1 m in diameter and always sits centred in a 1x1 m cell, so in
    a W x L room the valid positions are X = 0..W-1 and Y = 0..L-1, with [0,0]
    being the most south-west position (as in the assignment picture).
  * The room must be at least 1 x 1 m. Zero or negative sizes are rejected.

Movement
  * F/B move exactly 1 m forward/backward along the current heading; the
    heading does not change when reversing. (Letters and distances are the
    defaults and can be changed in the configuration.)
  * L/R rotate 90 degrees on the spot. Turning never hits a wall since the car
    is round and stays in the same cell.
  * A wall hit is detected when a move would take the car to a position
    outside the room. The simulation stops at the first such command; the
    remaining commands are not executed.
  * When unsuccessful, the output shows which command failed (1-based number),
    where the car tried to go, and the last valid position/heading.

Input handling
  * Values are separated by one or more spaces/tabs; leading/trailing
    whitespace is ignored.
  * Heading letters and command letters are case insensitive (n == N, f == F).
  * Whitespace inside the command string is ignored ("FF R" == "FFR").
  * An empty command line is valid and means "no commands"; the simulation is
    then successful and the car ends where it started.
  * The start position must be inside the room.
  * Invalid input is reported with a descriptive message and the user is
    asked again for that same value (earlier, valid answers are kept).
  * If the input stream ends (EOF) during a simulation the program stops with
    exit code 2. If it ends at the "run a new simulation?" question it is
    treated as "no" and the program exits normally.
  * The "run a new simulation?" question accepts Y/YES/N/NO (case
    insensitive); any other answer is rejected and the question is repeated.
  * Only the Monster Truck is simulated (per the assignment). Each new
    simulation starts from scratch (new room, start position and commands).


DESIGN
------
The code is split into small, single-responsibility parts (one folder each):

  Configuration/
               AppSettings (+ CommandSettings, LoggingSettings) are the
               strongly typed, validated settings. AppSettingsLoader builds
               them from the JSON files, environment variables and command
               line (Microsoft.Extensions.Configuration).
  Domain/      Value objects with no I/O:
               Heading (+ TurnLeft/TurnRight/letter conversion), Position,
               CarPose (position + heading) and Room (size + Contains check).
  Cars/        RcCar abstract base class holding the car's current pose, and
               MonsterTruck. Other models (Off-road, Sport track) can be added
               by deriving from RcCar.
  Commands/    Command pattern. IDriveCommand implementations (Forward, Back,
               TurnLeft, TurnRight) are pure functions CarPose -> CarPose;
               their letters, descriptions and distances come from
               CommandSettings.
               CommandParser maps letters to commands; new commands can be
               added by implementing IDriveCommand and registering them in
               the parser - no other code needs to change.
  Input/       InputParser validates the room and start position lines.
               InputFormatException carries a user friendly error message.
  Simulation/  Simulator runs the commands: it asks each command for the next
               pose, checks it against the Room and only then moves the car.
               The outcome is returned as a SimulationResult (success, or
               WallCollision details) instead of throwing, since hitting a
               wall is an expected result, not an error.
  UI/          SimulatorConsoleApp handles prompting/re-prompting, output and
               the "run a new simulation?" loop.
               It uses TextReader/TextWriter instead of Console directly so it
               can be tested end to end. ResultFormatter builds the result
               text. ExitCodes documents the process exit codes.
  Logging/     LoggingSetup builds the ILoggerFactory
               (Microsoft.Extensions.Logging) from LoggingSettings: the Debug
               provider plus FileLoggerProvider, a small thread-safe provider
               that appends to the log file. Log messages are source generated
               ([LoggerMessage]) next to the code that writes them.
  Program.cs   Composition root; loads the configuration, sets up logging,
               wires everything
               together and turns configuration errors into exit code 4 and
               any other unexpected exception into an error message + exit
               code 3 (both are logged too).

Error handling strategy
  * User input errors -> InputFormatException -> message shown, user re-asked.
  * Wall collision    -> regular SimulationResult (not an exception).
  * Programming errors (null arguments, invalid values passed in code, etc.)
    -> standard ArgumentException family, caught at the top level.

Tests
  tests\RcCars.Tests contains xUnit tests for the domain, commands, parsers,
  simulator, result formatting, configuration loading/validation, logging and
  the console application end to end
  (including re-prompting on invalid input and EOF handling).
