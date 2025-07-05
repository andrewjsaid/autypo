namespace Autypo;

/// <summary>
/// Specifies when Autypo loads and prepares its data source for search and completion operations.
/// </summary>
public enum InitializationMode
{
    /// <summary>
    /// Data is loaded synchronously before the tool becomes available for queries.
    /// Search and completion methods are guaranteed to have access to fully initialized data.
    /// </summary>
    Eager = 0,

    /// <summary>
    /// Data is loaded asynchronously in the background when the tool is first constructed.
    /// Synchronous search or completion methods may fail if invoked before loading completes,
    /// depending on the configured <see cref="UninitializedBehavior"/>.
    /// </summary>
    Background = 1,

    /// <summary>
    /// Data is loaded on-demand the first time it is needed.
    /// By default, multiple callers may trigger concurrent loads unless externally synchronized.
    /// </summary>
    Lazy = 2
}
