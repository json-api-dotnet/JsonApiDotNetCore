using System.Collections;
using System.Collections.Concurrent;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Captures the value of an attribute at a point in time, in a form that can be compared later using <see cref="object.Equals(object)" />. The structure
/// of compound attributes is taken from the resource graph metadata, so only members that are exposed as attributes take part in the comparison. Values
/// that are not compound and not a collection are captured as-is, so they keep their own equality semantics.
/// </summary>
internal static class AttributeValueSnapshot
{
    private static readonly ConcurrentDictionary<Type, bool> IsSetTypeCache = new();

    public static object? Create(AttrAttribute attribute, object instance)
    {
        object? value = attribute.GetValue(instance);
        return CreateFromValue(attribute, value);
    }

    private static object? CreateFromValue(AttrAttribute attribute, object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (attribute.IsCompound)
        {
            return attribute.IsCollection
                ? CreateCollection(value, element => element == null ? null : new CompoundSnapshot(attribute.CompoundType!, element))
                : new CompoundSnapshot(attribute.CompoundType!, value);
        }

        // A byte array is a collection according to the metadata, but is compared by content as a whole.
        if (attribute.IsCollection && value is IEnumerable and not byte[])
        {
            return CreateCollection(value, CreateScalar);
        }

        return CreateScalar(value);
    }

    private static CollectionSnapshot CreateCollection(object value, Func<object?, object?> elementFactory)
    {
        List<object?> items = [];

        foreach (object? element in (IEnumerable)value)
        {
            items.Add(elementFactory(element));
        }

        return new CollectionSnapshot(items, IsSet(value.GetType()));
    }

    private static object? CreateScalar(object? value)
    {
        return value is byte[] bytes ? new ByteArraySnapshot((byte[])bytes.Clone()) : value;
    }

    private static bool IsSet(Type type)
    {
        return IsSetTypeCache.GetOrAdd(type, static collectionType => collectionType.GetInterfaces().Append(collectionType).Any(IsSetInterface));
    }

    private static bool IsSetInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        Type definition = type.GetGenericTypeDefinition();
        return definition == typeof(ISet<>) || definition == typeof(IReadOnlySet<>);
    }

    private static bool ElementsEqual(IReadOnlyList<object?> left, IReadOnlyList<object?> right)
    {
        for (int index = 0; index < left.Count; index++)
        {
            if (!Equals(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class ByteArraySnapshot(byte[] bytes) : IEquatable<ByteArraySnapshot>
    {
        private readonly byte[] _bytes = bytes;

        public bool Equals(ByteArraySnapshot? other)
        {
            return other != null && _bytes.AsSpan().SequenceEqual(other._bytes);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ByteArraySnapshot);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.AddBytes(_bytes);
            return hash.ToHashCode();
        }
    }

    private sealed class CompoundSnapshot : IEquatable<CompoundSnapshot>
    {
        private readonly CompoundType _compoundType;
        private readonly object?[] _members;

        public CompoundSnapshot(CompoundType compoundType, object instance)
        {
            _compoundType = compoundType;
            _members = new object?[compoundType.Attributes.Count];

            int index = 0;

            foreach (AttrAttribute member in compoundType.Attributes)
            {
                _members[index++] = Create(member, instance);
            }
        }

        public bool Equals(CompoundSnapshot? other)
        {
            return other != null && Equals(_compoundType, other._compoundType) && ElementsEqual(_members, other._members);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CompoundSnapshot);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (object? member in _members)
            {
                hash.Add(member);
            }

            return hash.ToHashCode();
        }
    }

    private sealed class CollectionSnapshot(List<object?> items, bool isSet) : IEquatable<CollectionSnapshot>
    {
        private static readonly object NullKey = new();

        private readonly List<object?> _items = items;
        private readonly bool _isSet = isSet;

        public bool Equals(CollectionSnapshot? other)
        {
            if (other == null || _items.Count != other._items.Count)
            {
                return false;
            }

            return _isSet && other._isSet ? UnorderedEquals(other) : ElementsEqual(_items, other._items);
        }

        private bool UnorderedEquals(CollectionSnapshot other)
        {
            var remaining = new Dictionary<object, int>();

            foreach (object? item in _items)
            {
                object key = item ?? NullKey;
                remaining[key] = remaining.GetValueOrDefault(key) + 1;
            }

            foreach (object? item in other._items)
            {
                object key = item ?? NullKey;

                if (!remaining.TryGetValue(key, out int count) || count == 0)
                {
                    return false;
                }

                remaining[key] = count - 1;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CollectionSnapshot);
        }

        public override int GetHashCode()
        {
            // Must be consistent with unordered equality, so the hash is independent of element order.
            int hash = _items.Count;

            foreach (object? item in _items)
            {
                hash += item?.GetHashCode() ?? 0;
            }

            return hash;
        }
    }
}
