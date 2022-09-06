using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class NullabilityTests
{
    [Fact]
    public void Nullability_of_generated_types_is_as_expected()
    {
        PropertyInfo[] propertyInfos = typeof(ChickenAttributesInResponse).GetProperties();

        PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(ChickenAttributesInResponse.Name));
        propertyInfo.Should().BeNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(ChickenAttributesInResponse.NameOfCurrentFarm));
        propertyInfo.Should().BeNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(ChickenAttributesInResponse.Age));
        propertyInfo.Should().BeNonNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(ChickenAttributesInResponse.TimeAtCurrentFarmInDays));
        propertyInfo.Should().BeNullable();
    }
}
