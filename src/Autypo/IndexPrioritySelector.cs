namespace Autypo;

/// <summary>
/// Delegate that determines the dynamic priority of an index at query time.
/// Higher values indicate higher priority and will be evaluated earlier during search.
/// </summary>
/// <param name="searchContext">The context of the current search.</param>
/// <returns>
/// An integer priority value. Indexes with higher values are searched before lower-priority ones.
/// </returns>
public delegate int IndexPrioritySelector(AutypoSearchContext searchContext);
