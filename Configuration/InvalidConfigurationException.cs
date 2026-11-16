namespace RcCars.Configuration;

/// <summary>
/// Thrown when the configuration files, environment variables or command line
/// arguments cannot be read or contain invalid values.
/// </summary>
public sealed class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
