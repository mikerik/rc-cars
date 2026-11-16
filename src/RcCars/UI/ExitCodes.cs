namespace RcCars.UI;

/// <summary>Process exit codes, useful when the simulator is run from scripts.</summary>
public static class ExitCodes
{
    public const int Success = 0;
    public const int HitWall = 1;
    public const int InputAborted = 2;
    public const int UnexpectedError = 3;
}
