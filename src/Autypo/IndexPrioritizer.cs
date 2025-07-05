namespace Autypo;

/// <summary>
/// Provides ordered access to a collection of index readers based on
/// evaluated dynamic or static priorities.
/// </summary>
/// <typeparam name="T">The type of the indexed documents.</typeparam>
internal sealed class IndexPrioritizer<T>
{
    private readonly Entry[] _priorities;

    private int _nextEntryIndex;
    private int _lastPriority = int.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexPrioritizer{T}"/> class
    /// with the specified priority entries.
    /// </summary>
    /// <param name="priorities">The evaluated index reader priorities.</param>
    internal IndexPrioritizer(Entry[] priorities)
    {
        _priorities = priorities;
    }

    /// <summary>
    /// Constructs an <see cref="IndexPrioritizer{T}"/> by evaluating the priority of each index reader
    /// using its configured <see cref="IndexPrioritySelector"/> against the current search context.
    /// </summary>
    /// <param name="readers">The index readers to evaluate.</param>
    /// <param name="searchContext">The current search context.</param>
    /// <returns>
    /// An <see cref="IndexPrioritizer{T}"/> instance that exposes the readers in priority order.
    /// </returns>
    public static IndexPrioritizer<T> Create(IndexReader<T>[] readers, AutypoSearchContext searchContext)
    {
        var priorities = new Entry[readers.Length];
        for (var i = 0; i < readers.Length; i++)
        {
            priorities[i] = new(i, readers[i].GetPriority(searchContext));
        }

        Array.Sort(priorities);
        return new ( priorities);
    }

    /// <summary>
    /// Retrieves the next index reader to be evaluated, respecting configured priority tiers.
    /// If a match has already been found and the next reader belongs to a lower-priority tier,
    /// the iteration stops early to preserve ordering.
    /// </summary>
    /// <param name="hasAnyMatches">
    /// Indicates whether any previous reader has already yielded results.
    /// Set to <c>false</c> on the initial call.
    /// </param>
    /// <param name="readerIndex">
    /// When this method returns <c>true</c>, contains the index of the next reader to evaluate.
    /// </param>
    /// <returns>
    /// <c>true</c> if another reader should be evaluated; otherwise <c>false</c>.
    /// </returns>
    public bool GetNext(bool hasAnyMatches, out int readerIndex)
    {
        if (_nextEntryIndex >= _priorities.Length)
        {
            readerIndex = default;
            return false;
        }

        var nextEntry = _priorities[_nextEntryIndex++];

        if (_lastPriority == nextEntry.Priority || !hasAnyMatches)
        {
            _lastPriority = nextEntry.Priority;
            readerIndex = nextEntry.Index;
            return true;
        }

        readerIndex = default;
        return false;
    }

    /// <summary>
    /// Represents a sortable pair of an index reader and its evaluated priority.
    /// Used internally to determine evaluation order.
    /// </summary>
    internal readonly struct Entry(int index, int priority) : IComparable<Entry>
    {
        /// <summary>
        /// Represents a sortable pair of an index reader and its evaluated priority.
        /// Used internally to determine evaluation order.
        /// </summary>
        public int Index { get; } = index;

        /// <summary>
        /// The priority value assigned to this reader for the current query.
        /// </summary>
        public int Priority { get; } = priority;

        /// <inheritdoc />
        public int CompareTo(Entry other) => other.Priority.CompareTo(Priority);
    }
}
