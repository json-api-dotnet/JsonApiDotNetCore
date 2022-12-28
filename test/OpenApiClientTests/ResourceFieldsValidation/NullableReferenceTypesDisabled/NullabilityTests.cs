using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(ChickenAttributesInResponse.Name), NullabilityState.Unknown)]
    [InlineData(nameof(ChickenAttributesInResponse.NameOfCurrentFarm), NullabilityState.Unknown)]
    [InlineData(nameof(ChickenAttributesInResponse.Age), NullabilityState.NotNull)]
    [InlineData(nameof(ChickenAttributesInResponse.Weight), NullabilityState.NotNull)]
    [InlineData(nameof(ChickenAttributesInResponse.TimeAtCurrentFarmInDays), NullabilityState.Nullable)]
    [InlineData(nameof(ChickenAttributesInResponse.HasProducedEggs), NullabilityState.NotNull)]
    public void Nullability_of_generated_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        PropertyInfo[] properties = typeof(ChickenAttributesInResponse).GetProperties();
        PropertyInfo property = properties.Single(property => property.Name == propertyName);
        property.Should().HaveNullabilityState(expectedState);
    }
}
