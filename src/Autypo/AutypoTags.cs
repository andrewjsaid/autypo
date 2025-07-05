using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Autypo;

/// <summary>
/// Represents a dynamic set of key-value metadata tags associated with a token.
/// </summary>
/// <remarks>
/// Tags are typically used by token transformers or scoring logic to associate additional context
/// with a token. Keys are case-sensitive. Tag storage is optimized for small tag counts.
/// </remarks>
public struct AutypoTags
{
    private AutypoTag[]? _state;
    private int _count;

    /// <summary>
    /// Returns a default-initialized <see cref="AutypoTags"/> with no entries.
    /// </summary>
    public static AutypoTags None => default;

    /// <summary>
    /// Attempts to retrieve a tag value associated with the specified key and cast it to the given type.
    /// </summary>
    /// <typeparam name="T">The expected type of the tag value.</typeparam>
    /// <param name="key">The tag key to retrieve.</param>
    /// <param name="result">
    /// When this method returns, contains the value associated with the specified key, if found and castable.
    /// </param>
    /// <returns><c>true</c> if a tag with the specified key was found; otherwise, <c>false</c>.</returns>
    public readonly bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T result)
    {
        if (TryGetValue(key, out var resultObject))
        {
            result = (T)resultObject;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve a tag value associated with the specified key.
    /// </summary>
    /// <param name="key">The tag key to retrieve.</param>
    /// <param name="result">
    /// When this method returns, contains the value associated with the specified key, if found.
    /// </param>
    /// <returns><c>true</c> if a tag with the specified key was found; otherwise, <c>false</c>.</returns>
    public readonly bool TryGetValue(string key, [MaybeNullWhen(false)] out object result)
    {
        if (_state is null)
        {
            result = null;
            return false;
        }

        foreach (var tag in _state)
        {
            if (tag.Key == key)
            {
                result = tag.Value;
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets a read-only span of all tags currently set on this instance.
    /// </summary>
    public readonly ReadOnlySpan<AutypoTag> Values
    {
        get
        {
            if (_state is null)
            {
                return [];
            }

            return _state.AsSpan(0, _count);
        }
    }

    /// <summary>
    /// Sets or replaces the value for a given key.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <remarks>
    /// If the key already exists, its value is replaced.
    /// </remarks>
    public void Set(string key, object value)
    {
        if (_state is null)
        {
            Debug.Assert(_count is 0);
            _state = [new AutypoTag(key, value)];
            _count = 1;
            return;
        }

        for (var i = 0; i < _count; i++)
        {
            if (_state[i].Key == key)
            {
                _state[i] = new AutypoTag(key, value); // overwrite existing
                return;
            }
        }

        if (_count >= _state.Length)
        {
            var nextState = new AutypoTag[_state.Length * 2];
            _state.CopyTo(nextState, 0);
            _state = nextState;
        }

        _state[_count++] = new AutypoTag(key, value);
    }

    /// <summary>
    /// Removes the tag associated with the specified key, if present.
    /// </summary>
    /// <param name="key">The tag key to remove.</param>
    /// <returns><c>true</c> if a tag was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string key)
    {
        if (_state is null || _count == 0)
        {
            return false;
        }

        var writeIndex = 0;
        var removed = false;

        for (var readIndex = 0; readIndex < _count; readIndex++)
        {
            var tag = _state[readIndex];
            if (tag.Key == key)
            {
                removed = true;
                continue; // skip copying this one
            }

            _state[writeIndex++] = tag;
        }

        if (removed)
        {
            Array.Clear(_state, writeIndex, _count - writeIndex);
            _count = writeIndex;
        }

        return removed;
    }

    /// <summary>
    /// Removes all tags from this instance.
    /// </summary>
    public void Clear()
    {
        if (_state is not null)
        {
            Array.Clear(_state, 0, _count);
        }
        _count = 0;
    }

    /// <summary>
    /// Copies all tags from another <see cref="AutypoTags"/> into this instance, replacing any duplicates.
    /// </summary>
    /// <param name="other">The source from which to copy tags.</param>
    public void CopyFrom(AutypoTags other)
    {
        foreach(var tag in other.Values)
        {
            Set(tag.Key, tag.Value);
        }
    }

    /// <summary>
    /// Creates a shallow copy of the current tag set.
    /// </summary>
    /// <returns>A new <see cref="AutypoTags"/> instance with the same tags.</returns>
    public AutypoTags Clone()
    {
        var result = new AutypoTags();
        result._state = (AutypoTag[]?)_state?.Clone();
        result._count = _count;
        return result;
    }
}

/// <summary>
/// Represents a single tag as a key-value pair.
/// </summary>
public readonly struct AutypoTag(string key, object value)
{
    /// <summary>
    /// The name of the tag.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// The associated value of the tag.
    /// </summary>
    public object Value { get; } = value;
}
