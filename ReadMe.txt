RC CAR SIMULATOR - MONSTER TRUCK
================================

A console application that simulates a radio controlled Monster Truck driving
around a rectangular room and reports whether it can execute a series of
commands without hitting a wall.

Written in C# for .NET 10 (Windows or Linux).

(Build instructions and design notes are added as the solution takes shape.)


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
  * If the input stream ends (EOF) before all values have been entered the
    program stops with a separate exit code.
  * Only the Monster Truck is simulated (per the assignment).

The sample input from the assignment
  Room 8 6, start 2 3 N, commands FFFRFFLBB.
  With the interpretation above the room is 8 m wide (X 0-7) and 6 m long
  (Y 0-5). Tracing it by hand:
    start [2,3] N -> F [2,4] -> F [2,5] -> F [2,6]  outside the room!
  So the sample is an unsuccessful run: the car hits the north wall on the
  third command. (Had the room been 6 wide and 8 long, it would end at 4 4 N.)
  The same commands from start 2 1 N succeed and end at 4 2 N.
