using System.Reflection;
using FluentAssertions;
using FluentAssertions.Types;

namespace OpenApiClientTests;

internal static class PropertyInfoAssertionsExtensions
{
    [CustomAssertion]
    public static void HaveNullabilityState(this PropertyInfoAssertions source, NullabilityState expected, string because = "", params object[] becauseArgs)
    {
        PropertyInfo propertyInfo = source.Subject;

        NullabilityInfoContext nullabilityContext = new();
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(propertyInfo);

        nullabilityInfo.ReadState.Should().Be(expected, because, becauseArgs);
        nullabilityInfo.WriteState.Should().Be(expected, because, becauseArgs);
    }
}
