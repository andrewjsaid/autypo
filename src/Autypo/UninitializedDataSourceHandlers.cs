namespace Autypo;

/// <summary>
/// Provides built-in fallback handlers for uninitialized data scenarios.
/// </summary>
/// <remarks>
/// These handlers support <see cref="UninitializedBehavior.Throw"/> and <see cref="UninitializedBehavior.ReturnEmpty"/>.
/// </remarks>
internal static class UninitializedDataSourceHandlers
{
    /// <summary>
    /// Returns a handler that throws an exception when invoked, indicating
    /// that search was called before initialization completed.
    /// </summary>
    public static UninitializedDataSourceHandler<T> Throw<T>() where T : notnull
    {
        return static (_, _) => throw new InvalidOperationException(Resources.Search_Uninitialized);
    }

    /// <summary>
    /// Returns a handler that always yields an empty result set when invoked.
    /// </summary>
    public static UninitializedDataSourceHandler<T> ReturnEmpty<T>() where T : notnull
    {
        return static (_, _) => [];
    }
}
