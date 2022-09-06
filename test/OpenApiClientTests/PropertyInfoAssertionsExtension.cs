using System.Reflection;
using FluentAssertions;
using FluentAssertions.Types;

namespace OpenApiClientTests;

internal static class PropertyInfoAssertionsExtensions
{
    [CustomAssertion]
    public static void BeNullable(this PropertyInfoAssertions source, string because = "", params object[] becauseArgs)
    {
        MemberInfo memberInfo = source.Subject;

        TypeCategory typeCategory = memberInfo.GetTypeCategory();

        typeCategory.Should().Match(category => category == TypeCategory.NullableReferenceType || category == TypeCategory.NullableValueType, because,
            becauseArgs);
    }

    [CustomAssertion]
    public static void BeNonNullable(this PropertyInfoAssertions source, string because = "", params object[] becauseArgs)
    {
        MemberInfo memberInfo = source.Subject;

        TypeCategory typeCategory = memberInfo.GetTypeCategory();

        typeCategory.Should().Match(category => category == TypeCategory.NonNullableReferenceType || category == TypeCategory.ValueType, because, becauseArgs);
    }
}
