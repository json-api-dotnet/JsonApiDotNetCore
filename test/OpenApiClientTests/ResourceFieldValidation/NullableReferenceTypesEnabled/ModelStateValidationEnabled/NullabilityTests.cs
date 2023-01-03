using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesEnabled.ModelStateValidationEnabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesEnabled.ModelStateValidationEnabled;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), NullabilityState.NotNull)]
    public void Nullability_of_generated_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        PropertyInfo[] properties = typeof(ResourceAttributesInPostRequest).GetProperties();
        PropertyInfo property = properties.Single(property => property.Name == propertyName);
        property.Should().HaveNullabilityState(expectedState);
    }
}
