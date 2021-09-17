using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using JetBrains.Annotations;

namespace TestBuildingBlocks
{
    [PublicAPI]
    public static class ObjectAssertionsExtensions
    {
        private const decimal NumericPrecision = 0.00000000001M;
        private static readonly TimeSpan TimePrecision = TimeSpan.FromMilliseconds(20);

        private static readonly JsonWriterOptions JsonWriterOptions = new()
        {
            Indented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Same as <see cref="DateTimeAssertions{TAssertions}.BeCloseTo(DateTime, TimeSpan, string, object[])" />, but with default precision.
        /// </summary>
        [CustomAssertion]
        public static AndConstraint<TAssertions> BeCloseTo<TAssertions>(this DateTimeAssertions<TAssertions> parent, DateTime nearbyTime, string because = "",
            params object[] becauseArgs)
            where TAssertions : DateTimeAssertions<TAssertions>
        {
            return parent.BeCloseTo(nearbyTime, TimePrecision, because, becauseArgs);
        }

        /// <summary>
        /// Same as <see cref="DateTimeOffsetAssertions{TAssertions}.BeCloseTo(DateTimeOffset, TimeSpan, string, object[])" />, but with default precision.
        /// </summary>
        [CustomAssertion]
        public static AndConstraint<TAssertions> BeCloseTo<TAssertions>(this DateTimeOffsetAssertions<TAssertions> parent, DateTimeOffset nearbyTime,
            string because = "", params object[] becauseArgs)
            where TAssertions : DateTimeOffsetAssertions<TAssertions>
        {
            return parent.BeCloseTo(nearbyTime, TimePrecision, because, becauseArgs);
        }

        /// <summary>
        /// Same as <see cref="NumericAssertionsExtensions.BeApproximately(NumericAssertions{decimal}, decimal, decimal, string, object[])" />, but with default
        /// precision.
        /// </summary>
        [CustomAssertion]
        public static AndConstraint<NumericAssertions<decimal>> BeApproximately(this NumericAssertions<decimal> parent, decimal expectedValue,
            string because = "", params object[] becauseArgs)
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
        /// Used to assert on a JSON-formatted string, ignoring differences in insignificant whitespace and line endings.
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
            var writer = new Utf8JsonWriter(stream, JsonWriterOptions);

            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
