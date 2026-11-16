RC CAR SIMULATOR - MONSTER TRUCK
================================

A console application that simulates a radio controlled Monster Truck driving
around a rectangular room and reports whether it can execute a series of
commands without hitting a wall.

Written in C# for .NET 10 (Windows or Linux).


HOW TO BUILD AND RUN
--------------------
Requirements: .NET 10 SDK.

  Visual Studio 2026: open RcCars.slnx and press F5.
                      Run the unit tests from Test Explorer.

  Command line (from the repository root):
    dotnet build
    dotnet run --project src/RcCars
    dotnet test

For each simulation the simulator asks for three lines of input:

  1. Room size:              <width> <length>        e.g.  8 6
  2. Start position/heading: <x> <y> <N|E|S|W>       e.g.  2 3 N
  3. Commands:               any sequence of F/B/L/R e.g.  FFFRFFLBB

After the result has been printed the simulator asks whether you want to run
a new simulation. Answer Y (yes) to start over with a new room, or N (no) to
exit the simulator.

Input can also be piped, e.g. (PowerShell):
    "8 6", "2 3 N", "FFFRFFLBB" | dotnet run --project src/RcCars

Exit codes (useful when the simulator is scripted):
  0 = the last simulation was successful
  1 = the car drove into a wall in the last simulation
  2 = input ended in the middle of a simulation (e.g. Ctrl+Z / Ctrl+D)
  3 = unexpected internal error


EXAMPLE
-------
The sample from the assignment (room 8 6, start 2 3 N, commands FFFRFFLBB)
results in a collision: the room is 6 meters long so the valid Y coordinates
are 0-5, and the third "F" would move the car from [2, 5] to [2, 6]. Output:

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
    This matches the order of the start position (x before y).
  * The car is 1 m in diameter and always sits centred in a 1x1 m cell, so in
    a W x L room the valid positions are X = 0..W-1 and Y = 0..L-1, with [0,0]
    being the most south-west position (as in the assignment picture).
  * The room must be at least 1 x 1 m. Zero or negative sizes are rejected.

Movement
  * F/B move exactly 1 m forward/backward along the current heading; the
    heading does not change when reversing.
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
  * The whole command string is validated before the simulation starts; an
    unknown letter is an input error, not a failed simulation.
  * Invalid input is reported with a descriptive message and the user is
    asked again for that same value (earlier, valid answers are kept).
  * If the input stream ends (EOF) during a simulation the program stops with
    exit code 2. If it ends at the "run a new simulation?" question it is
    treated as "no" and the program exits normally.
  * The "run a new simulation?" question accepts Y/YES/N/NO (case
    insensitive); any other answer is rejected and the question is repeated.
    Running several simulations in one session is an addition of mine; the
    assignment only asks for one.
  * Only the Monster Truck is simulated (per the assignment). Each new
    simulation starts from scratch (new room, start position and commands).


DESIGN
------
The code is split into small, single-responsibility parts (one folder each
in src/RcCars):

  Domain/      Value objects with no I/O: Heading (rotation and letter
               conversion), Position, CarPose (position + heading) and Room
               (size + Contains check).
  Commands/    Command pattern. The IDriveCommand implementations (Forward,
               Back, TurnLeft, TurnRight) are pure functions CarPose ->
               CarPose, so a move can be checked before it is applied.
               CommandParser maps letters to commands. A new command is one
               new class plus one entry in CommandParser.Default.
  Cars/        RcCar base class holding the car's current pose, and
               MonsterTruck. Other models (Off-road, Sport track) would derive
               from RcCar. I deliberately did not guess how they would differ.
  Input/       InputParser validates the room, start position and yes/no
               lines. InputFormatException carries a message meant for the
               user.
  Simulation/  Simulator runs the commands: it asks each command for the next
               pose, checks it against the Room and only then moves the car.
               The outcome is returned as a SimulationResult (success, or
               WallCollision details).
  UI/          SimulatorConsoleApp handles prompting/re-prompting, output and
               the "run a new simulation?" loop. It uses TextReader/TextWriter
               instead of Console directly so it can be tested end to end.
               ResultFormatter builds the result text. ExitCodes lists the
               process exit codes.
  Program.cs   Composition root: wires everything together and turns any
               unexpected exception into an error message and exit code 3.

Error handling strategy
  * Invalid user input -> InputFormatException -> message shown, user asked
    again.
  * Wall collision     -> a regular SimulationResult, not an exception: hitting
    a wall is an expected outcome of a test run.
  * Programming errors (null arguments, invalid values passed in code, etc.)
    -> guard clauses throwing the standard ArgumentException family, caught
    at the top level in Program.cs.

Deliberately left out
  * Configuration (command letters, distances) and logging. Nothing in the
    assignment asks for them, the command letters are part of the input
    format, and the program is small enough to understand without logs.
  * Behaviour for other car models: only the extension point (RcCar) exists.

Build settings shared by all projects (Directory.Build.props): nullable
reference types enabled, warnings treated as errors, code style enforced in
the build (.editorconfig).


TESTS
-----
tests/RcCars.Tests contains xUnit tests for the domain, commands, parsers,
simulator and the console application end to end (re-prompting on invalid
input, EOF handling and exit codes). Every commit in the history builds and
its tests pass.
