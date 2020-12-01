using System;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests
{
    public static class ObjectAssertionsExtensions
    {
        /// <summary>
        /// Used to assert on a nullable <see cref="DateTimeOffset"/> column, whose value is returned as <see cref="DateTime"/> in JSON:API response body.
        /// </summary>
        public static void BeCloseTo(this ObjectAssertions source, DateTimeOffset? expected, string because = "",
            params object[] becauseArgs)
        {
            if (expected == null)
            {
                source.Subject.Should().BeNull(because, becauseArgs);
            }
            else
            {
                // We lose a little bit of precision (milliseconds) on roundtrip through PostgreSQL database.

                var value = new DateTimeOffset((DateTime) source.Subject);
                value.Should().BeCloseTo(expected.Value, because: because, becauseArgs: becauseArgs);
            }
        }

        /// <summary>
        /// Used to assert on a <see cref="decimal"/> column, whose value is returned as <see cref="double"/> in json:api response body.
        /// </summary>
        public static void BeApproximately(this ObjectAssertions source, decimal? expected, decimal precision = 0.00000000001M, string because = "",
            params object[] becauseArgs)
        {
            // We lose a little bit of precision on roundtrip through PostgreSQL database.

            var value = (decimal?) (double) source.Subject;
            value.Should().BeApproximately(expected, precision, because, becauseArgs);
        }
    }
}
