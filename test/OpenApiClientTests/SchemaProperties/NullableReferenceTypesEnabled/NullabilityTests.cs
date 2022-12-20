using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class NullabilityTests
{
    [Theory]
    [InlineData(nameof(CowAttributesInResponse.Name), NullabilityState.NotNull)]
    [InlineData(nameof(CowAttributesInResponse.NameOfCurrentFarm), NullabilityState.NotNull)]
    [InlineData(nameof(CowAttributesInResponse.NameOfPreviousFarm), NullabilityState.Nullable)]
    [InlineData(nameof(CowAttributesInResponse.Nickname), NullabilityState.NotNull)]
    [InlineData(nameof(CowAttributesInResponse.Age), NullabilityState.NotNull)]
    [InlineData(nameof(CowAttributesInResponse.Weight), NullabilityState.NotNull)]
    [InlineData(nameof(CowAttributesInResponse.TimeAtCurrentFarmInDays), NullabilityState.Nullable)]
    [InlineData(nameof(CowAttributesInResponse.HasProducedMilk), NullabilityState.NotNull)]
    public void Nullability_of_generated_property_is_as_expected(string propertyName, NullabilityState expectedState)
    {
        PropertyInfo[] properties = typeof(CowAttributesInResponse).GetProperties();
        PropertyInfo property = properties.Single(property => property.Name == propertyName);
        property.Should().HaveNullabilityState(expectedState);
    }
}
