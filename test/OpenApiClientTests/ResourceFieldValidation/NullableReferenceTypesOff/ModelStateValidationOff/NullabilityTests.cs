using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), NullabilityState.Unknown)]
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
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToOne), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), NullabilityState.Unknown)]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), NullabilityState.Unknown)]
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
