using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

// ReSharper disable UnusedMethodReturnValue.Global

namespace TestBuildingBlocks;

public static class FluentExtensions
{
    private const decimal NumericPrecision = 0.00000000001M;

    private static readonly JsonWriterOptions JsonWriterOptions = new()
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Same as <see cref="NumericAssertionsExtensions.BeApproximately(NumericAssertions{decimal}, decimal, decimal, string, object[])" />, but with default
    /// precision.
    /// </summary>
    [CustomAssertion]
    public static AndConstraint<NumericAssertions<decimal>> BeApproximately(this NumericAssertions<decimal> parent, decimal expectedValue, string because = "",
        params object[] becauseArgs)
    {
        return parent.BeApproximately(expectedValue, NumericPrecision, because, becauseArgs);
    }

    /// <summary>
    /// Same as <see cref="NumericAssertionsExtensions.BeApproximately(NullableNumericAssertions{decimal}, decimal?, decimal, string, object[])" />, but with
    /// default precision.
    /// </summary>
    [CustomAssertion]
    public static AndConstraint<NullableNumericAssertions<decimal>> BeApproximately(this NullableNumericAssertions<decimal> parent, decimal? expectedValue,
        string because = "", params object[] becauseArgs)
    {
        return parent.BeApproximately(expectedValue, NumericPrecision, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that a JSON-formatted string matches the specified expected one, ignoring differences in insignificant whitespace and line endings.
    /// </summary>
    [CustomAssertion]
    public static void BeJson(this StringAssertions source, string expected, string because = "", params object[] becauseArgs)
    {
        using JsonDocument sourceJson = JsonDocument.Parse(source.Subject);
        using JsonDocument expectedJson = JsonDocument.Parse(expected);

        string sourceText = ToJsonString(sourceJson);
        string expectedText = ToJsonString(expectedJson);

        sourceText.Should().Be(expectedText, because, becauseArgs);
    }

    private static string ToJsonString(JsonDocument document)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, JsonWriterOptions))
        {
            document.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

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
