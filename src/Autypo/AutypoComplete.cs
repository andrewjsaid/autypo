namespace Autypo;

/// <summary>
/// Defines an interface for providing autocomplete suggestions based on partial user input.
/// </summary>
internal sealed class AutypoComplete(IAutypoSearch<string> inner) : IAutypoComplete
{
    /// <inheritdoc />
    public IEnumerable<string> Complete(string term, AutypoSearchContext? context = null)
    {
        return inner.Search(term, context).Select(r => r.Value);
    }

    /// <inheritdoc />
    public async ValueTask<IEnumerable<string>> CompleteAsync(string term, AutypoSearchContext? context = null, CancellationToken cancellationToken = default)
    {
        return (await inner.SearchAsync(term, context, cancellationToken)).Select(r => r.Value);
    }
}