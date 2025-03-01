using FluentAssertions;
using FluentAssertions.Collections;

namespace TestBuildingBlocks;

public static class FluentExtensions
{
    public static AndConstraint<TAssertions> OnlyContainKeys<TCollection, TKey, TValue, TAssertions>(
        this GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions> source, params TKey[] expected)
        where TCollection : IEnumerable<KeyValuePair<TKey, TValue>>
        where TAssertions : GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions>
    {
        return source.HaveCount(expected.Length).And.ContainKeys(expected);
    }
}
