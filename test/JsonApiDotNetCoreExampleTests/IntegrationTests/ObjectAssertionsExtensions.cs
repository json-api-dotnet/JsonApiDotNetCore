using System;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests
{
    public static class ObjectAssertionsExtensions
    {
        /// <summary>
        /// Used to assert on a nullable <see cref="DateTimeOffset"/> column, whose value is returned as <see cref="DateTime"/> in json:api response body.
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
    }
}
