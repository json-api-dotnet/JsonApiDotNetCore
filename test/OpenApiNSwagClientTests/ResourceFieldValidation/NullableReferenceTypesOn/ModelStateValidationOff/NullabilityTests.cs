using System.Reflection;
using FluentAssertions;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

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
    public void Nullability_of_generated_attribute_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        // Act
        PropertyInfo? property = typeof(ResourceAttributesInPostRequest).GetProperty(propertyName);

        // Assert
        property.ShouldNotBeNull();
        property.Should().HaveNullabilityState(expectedState);
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), NullabilityState.Nullable)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), NullabilityState.NotNull)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), NullabilityState.NotNull)]
    public void Nullability_of_generated_relationship_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        // Act
        PropertyInfo? relationshipProperty = typeof(ResourceRelationshipsInPostRequest).GetProperty(propertyName);

        // Assert
        relationshipProperty.ShouldNotBeNull();

        PropertyInfo? dataProperty = relationshipProperty.PropertyType.GetProperty("Data");
        dataProperty.ShouldNotBeNull();
        dataProperty.Should().HaveNullabilityState(expectedState);
    }
}
