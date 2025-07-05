namespace Autypo.Configuration;

/// <summary>
/// Represents errors that occur during configuration of an Autypo index or engine.
/// </summary>
/// <remarks>
/// This exception is typically thrown when required components are missing or when configuration
/// values are invalid for the given initialization strategy.
/// </remarks>
[Serializable]
public sealed class AutypoConfigurationException : Exception
{
    /// <inheritdoc />
    public AutypoConfigurationException() { }

    /// <inheritdoc />
    public AutypoConfigurationException(string message) : base(message) { }

    /// <inheritdoc />
    public AutypoConfigurationException(string message, Exception inner) : base(message, inner) { }
}
