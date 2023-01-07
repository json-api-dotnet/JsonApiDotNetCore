using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), NullabilityState.Nullable)]
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
