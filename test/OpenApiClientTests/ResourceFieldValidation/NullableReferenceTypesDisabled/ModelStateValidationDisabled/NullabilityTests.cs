using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesDisabled.ModelStateValidationDisabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesDisabled.ModelStateValidationDisabled;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), NullabilityState.Nullable)]
    public void Nullability_of_generated_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        PropertyInfo[] properties = typeof(ResourceAttributesInPostRequest).GetProperties();
        PropertyInfo property = properties.Single(property => property.Name == propertyName);
        property.Should().HaveNullabilityState(expectedState);
    }
}
