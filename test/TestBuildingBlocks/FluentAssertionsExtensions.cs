using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

// ReSharper disable UnusedMethodReturnValue.Global

namespace TestBuildingBlocks;

public static class FluentAssertionsExtensions
{
    // Workaround for source.Should().NotBeNull().And.Subject having declared type 'object'.
    [System.Diagnostics.Contracts.Pure]
    public static StrongReferenceTypeAssertions<T> RefShould<T>([SysNotNull] this T? actualValue)
        where T : class
    {
        actualValue.Should().NotBeNull();
        return new StrongReferenceTypeAssertions<T>(actualValue);
    }

    public static AndConstraint<TAssertions> OnlyContainKeys<TCollection, TKey, TValue, TAssertions>(
        this GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions> source, params TKey[] expected)
        where TCollection : IEnumerable<KeyValuePair<TKey, TValue>>
        where TAssertions : GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions>
    {
        return source.HaveCount(expected.Length).And.ContainKeys(expected);
    }

    // Workaround for CS0854: An expression tree may not contain a call or invocation that uses optional arguments.
    public static WhoseValueConstraint<TCollection, TKey, TValue, TAssertions> ContainKey2<TCollection, TKey, TValue, TAssertions>(
        this GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions> source, TKey expected)
        where TCollection : IEnumerable<KeyValuePair<TKey, TValue>>
        where TAssertions : GenericDictionaryAssertions<TCollection, TKey, TValue, TAssertions>
    {
        return source.ContainKey(expected);
    }

    public static void With<T>(this T subject, [InstantHandle] Action<T> continuation)
    {
        continuation(subject);
    }

    public sealed class StrongReferenceTypeAssertions<TReference>(TReference subject)
        : ReferenceTypeAssertions<TReference, StrongReferenceTypeAssertions<TReference>>(subject)
    {
        protected override string Identifier => "subject";
    }
}
