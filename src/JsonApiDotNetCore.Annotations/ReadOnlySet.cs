// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET8_0
#pragma warning disable

// ReadOnlySet<T> was introduced in .NET 9.
// This file was copied from https://github.com/dotnet/runtime/blob/release/9.0/src/libraries/System.Collections/src/System/Collections/Generic/ReadOnlySet.cs
// and made internal to enable usage on lower .NET versions.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.ObjectModel;

/// <summary>Represents a read-only, generic set of values.</summary>
/// <typeparam name="T">The type of values in the set.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[ExcludeFromCodeCoverage]
internal class ReadOnlySet<T> : IReadOnlySet<T>, ISet<T>, ICollection
{
    /// <summary>The wrapped set.</summary>
    private readonly ISet<T> _set;

    /// <summary>Initializes a new instance of the <see cref="ReadOnlySet{T}"/> class that is a wrapper around the specified set.</summary>
    /// <param name="set">The set to wrap.</param>
    public ReadOnlySet(ISet<T> set)
    {
        ArgumentNullException.ThrowIfNull(set);
        _set = set;
    }

    /// <summary>Gets an empty <see cref="ReadOnlySet{T}"/>.</summary>
    public static ReadOnlySet<T> Empty { get; } = new ReadOnlySet<T>(new HashSet<T>());

    /// <summary>Gets the set that is wrapped by this <see cref="ReadOnlySet{T}"/> object.</summary>
    protected ISet<T> Set => _set;

    /// <inheritdoc/>
    public int Count => _set.Count;

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() =>
        _set.Count == 0 ? ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator() :
            _set.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public bool Contains(T item) => _set.Contains(item);

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    /// <inheritdoc/>
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index) => CollectionHelpers.CopyTo(_set, array, index);

    /// <inheritdoc/>
    bool ICollection<T>.IsReadOnly => true;

    /// <inheritdoc/>
    bool ICollection.IsSynchronized => false;

    /// <inheritdoc/>
    object ICollection.SyncRoot => _set is ICollection c ? c.SyncRoot : this;

    /// <inheritdoc/>
    bool ISet<T>.Add(T item) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    /// <inheritdoc/>
    void ICollection<T>.Clear() => throw new NotSupportedException();

    /// <inheritdoc/>
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    private static class CollectionHelpers
    {
        private static void ValidateCopyToArguments(int sourceCount, Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Rank != 1)
            {
                throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("The lower bound of target array must be zero.", nameof(array));
            }

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, array.Length);

            if (array.Length - index < sourceCount)
            {
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            }
        }

        internal static void CopyTo<T>(ICollection<T> collection, Array array, int index)
        {
            ValidateCopyToArguments(collection.Count, array, index);

            if (collection is ICollection nonGenericCollection)
            {
                // Easy out if the ICollection<T> implements the non-generic ICollection
                nonGenericCollection.CopyTo(array, index);
            }
            else if (array is T[] items)
            {
                collection.CopyTo(items, index);
            }
            else
            {
                // We can't cast array of value type to object[], so we don't support widening of primitive types here.
                if (array is not object?[] objects)
                {
                    throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", nameof(array));
                }

                try
                {
                    foreach (T item in collection)
                    {
                        objects[index++] = item;
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", nameof(array));
                }
            }
        }
    }
}
#endif
