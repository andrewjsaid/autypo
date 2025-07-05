namespace Autypo;

/// <summary>
/// Specifies how Autypo behaves when a search or autocomplete operation is invoked
/// before the underlying data source has completed loading.
/// </summary>
/// <remarks>
/// This setting is relevant when using <see cref="InitializationMode.Lazy"/> or <see cref="InitializationMode.Background"/>.
/// It determines whether to fail, return an empty result set, or never allow uninitialized access (in eager mode).
/// </remarks>
public enum UninitializedBehavior
{
    /// <summary>
    /// Indicates that uninitialized access is not possible.
    /// Only valid in <see cref="InitializationMode.Eager"/> mode, where the data is guaranteed
    /// to be loaded before any search is allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// If a search is attempted before initialization completes,
    /// the search will return an empty result set.
    /// </summary>
    ReturnEmpty = 1,

    /// <summary>
    /// If a search is attempted before initialization completes,
    /// <see cref="AutypoUninitializedException"/> is thrown.
    /// This allows the caller to detect and handle this explicitly.
    /// </summary>
    Throw = 2,
}
