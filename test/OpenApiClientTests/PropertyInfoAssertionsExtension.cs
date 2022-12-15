using System.Reflection;
using FluentAssertions;
using FluentAssertions.Types;

namespace OpenApiClientTests;

internal static class PropertyInfoAssertionsExtensions
{
    [CustomAssertion]
    public static void BeNullable(this PropertyInfoAssertions source, string because = "", params object[] becauseArgs)
    {
        PropertyInfo propertyInfo = source.Subject;

        NullabilityInfoContext nullabilityContext = new();
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(propertyInfo);

        nullabilityInfo.ReadState.Should().NotBe(NullabilityState.NotNull, because, becauseArgs);
    }

    [CustomAssertion]
    public static void BeNonNullable(this PropertyInfoAssertions source, string because = "", params object[] becauseArgs)
    {
        PropertyInfo propertyInfo = source.Subject;

        NullabilityInfoContext nullabilityContext = new();
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(propertyInfo);

        nullabilityInfo.ReadState.Should().Be(NullabilityState.NotNull, because, becauseArgs);
    }
}
