namespace RcCars.Input;

/// <summary>
/// Thrown when user supplied input cannot be understood or is not valid.
/// The message is written so that it can be shown directly to the user.
/// </summary>
public sealed class InputFormatException : Exception
{
    public InputFormatException(string message)
        : base(message)
    {
    }
}
