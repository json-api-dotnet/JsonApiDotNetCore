using System.Reflection;
using FluentAssertions;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.NonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNonNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableReferenceType), NullabilityState.Nullable)]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableReferenceType), NullabilityState.NotNull)]
    [InlineData(nameof(AttributesInCreateResourceRequest.ValueType), NullabilityState.NotNull)]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredValueType), NullabilityState.NotNull)]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableValueType), NullabilityState.Nullable)]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableValueType), NullabilityState.NotNull)]
    public void Nullability_of_generated_attribute_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        // Act
        PropertyInfo? property = typeof(AttributesInCreateResourceRequest).GetProperty(propertyName);

        // Assert
        property.ShouldNotBeNull();
        property.Should().HaveNullabilityState(expectedState);
    }

    [Theory]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NonNullableToOne), NullabilityState.NotNull)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNonNullableToOne), NullabilityState.NotNull)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NullableToOne), NullabilityState.Nullable)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNullableToOne), NullabilityState.NotNull)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.ToMany), NullabilityState.NotNull)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredToMany), NullabilityState.NotNull)]
    public void Nullability_of_generated_relationship_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        // Act
        PropertyInfo? relationshipProperty = typeof(RelationshipsInCreateResourceRequest).GetProperty(propertyName);

        // Assert
        relationshipProperty.ShouldNotBeNull();

        PropertyInfo? dataProperty = relationshipProperty.PropertyType.GetProperty("Data");
        dataProperty.ShouldNotBeNull();
        dataProperty.Should().HaveNullabilityState(expectedState);
    }
}
