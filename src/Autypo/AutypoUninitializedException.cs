namespace Autypo;

/// <summary>
/// The exception that is thrown when a synchronous search operation is invoked before the Autypo index has been initialized.
/// </summary>
/// <remarks>
/// This exception only applies to the use of synchronous methods such as <c>Search</c> or <c>Complete</c>
/// when Autypo is configured for background or lazy initialization.
/// 
/// <para>
/// When using lazy or background indexing, there may be a delay before the index is fully built.
/// During this window, calling synchronous methods may result in an <see cref="AutypoUninitializedException"/>.
/// </para>
/// 
/// <para>
/// This exception is never thrown when using <c>SearchAsync</c> or <c>CompleteAsync</c>, which will wait for the index
/// to be ready or cancel via the provided <see cref="CancellationToken"/>.
/// </para>
/// </remarks>
[Serializable]
public class AutypoUninitializedException : Exception
{
    public AutypoUninitializedException() { }
    public AutypoUninitializedException(string message) : base(message) { }
    public AutypoUninitializedException(string message, Exception inner) : base(message, inner) { }
}
