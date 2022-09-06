using System.Reflection;
using FluentAssertions;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class NullabilityTests
{
    [Fact]
    public void Nullability_of_generated_types_is_as_expected()
    {
        PropertyInfo[] propertyInfos = typeof(CowAttributesInResponse).GetProperties();

        PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(CowAttributesInResponse.Name));
        propertyInfo.Should().BeNonNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(CowAttributesInResponse.NameOfPreviousFarm));
        propertyInfo.Should().BeNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(CowAttributesInResponse.Age));
        propertyInfo.Should().BeNonNullable();

        propertyInfo = propertyInfos.FirstOrDefault(property => property.Name == nameof(CowAttributesInResponse.TimeAtCurrentFarmInDays));
        propertyInfo.Should().BeNullable();
    }
}
