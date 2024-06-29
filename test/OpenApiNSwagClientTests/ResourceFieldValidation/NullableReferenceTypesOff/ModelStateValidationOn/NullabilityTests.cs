using System.Reflection;
using FluentAssertions;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.ReferenceType), NullabilityState.Unknown)]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredReferenceType), NullabilityState.Unknown)]
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
    [InlineData(nameof(RelationshipsInCreateResourceRequest.ToOne), NullabilityState.Unknown)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredToOne), NullabilityState.Unknown)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.ToMany), NullabilityState.Unknown)]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredToMany), NullabilityState.Unknown)]
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
