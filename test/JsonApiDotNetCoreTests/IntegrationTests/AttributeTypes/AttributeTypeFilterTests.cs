using System.Net;
using System.Reflection;
using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AttributeTypes;

public sealed class AttributeTypeFilterTests : IClassFixture<IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    private readonly IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;
    private readonly AttributeTypesFakers _fakers = new();

    public AttributeTypeFilterTests(IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TypeContainersController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestBoolean))]
    [InlineData(nameof(TypeContainer.TestNullableBoolean))]
    [InlineData(nameof(TypeContainer.TestByte))]
    [InlineData(nameof(TypeContainer.TestNullableByte))]
    [InlineData(nameof(TypeContainer.TestSignedByte))]
    [InlineData(nameof(TypeContainer.TestNullableSignedByte))]
    [InlineData(nameof(TypeContainer.TestInt16))]
    [InlineData(nameof(TypeContainer.TestNullableInt16))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt16))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt16))]
    [InlineData(nameof(TypeContainer.TestInt32))]
    [InlineData(nameof(TypeContainer.TestNullableInt32))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt32))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt32))]
    [InlineData(nameof(TypeContainer.TestInt64))]
    [InlineData(nameof(TypeContainer.TestNullableInt64))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt64))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt64))]
    [InlineData(nameof(TypeContainer.TestInt128))]
    [InlineData(nameof(TypeContainer.TestNullableInt128))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt128))]
    [InlineData(nameof(TypeContainer.TestBigInteger))]
    [InlineData(nameof(TypeContainer.TestNullableBigInteger))]
    [InlineData(nameof(TypeContainer.TestHalf))]
    [InlineData(nameof(TypeContainer.TestNullableHalf))]
    [InlineData(nameof(TypeContainer.TestFloat))]
    [InlineData(nameof(TypeContainer.TestNullableFloat))]
    [InlineData(nameof(TypeContainer.TestDouble))]
    [InlineData(nameof(TypeContainer.TestNullableDouble))]
    [InlineData(nameof(TypeContainer.TestDecimal))]
    [InlineData(nameof(TypeContainer.TestNullableDecimal))]
    [InlineData(nameof(TypeContainer.TestComplex))]
    [InlineData(nameof(TypeContainer.TestNullableComplex))]
    [InlineData(nameof(TypeContainer.TestChar))]
    [InlineData(nameof(TypeContainer.TestNullableChar))]
    [InlineData(nameof(TypeContainer.TestString))]
    [InlineData(nameof(TypeContainer.TestNullableString))]
    [InlineData(nameof(TypeContainer.TestRune))]
    [InlineData(nameof(TypeContainer.TestNullableRune))]
    [InlineData(nameof(TypeContainer.TestDateTimeOffset))]
    [InlineData(nameof(TypeContainer.TestNullableDateTimeOffset))]
    [InlineData(nameof(TypeContainer.TestDateTime))]
    [InlineData(nameof(TypeContainer.TestNullableDateTime))]
    [InlineData(nameof(TypeContainer.TestDateTimeStoredInLocalTimeZone))]
    [InlineData(nameof(TypeContainer.TestDateOnly))]
    [InlineData(nameof(TypeContainer.TestNullableDateOnly))]
    [InlineData(nameof(TypeContainer.TestTimeOnly))]
    [InlineData(nameof(TypeContainer.TestNullableTimeOnly))]
    [InlineData(nameof(TypeContainer.TestTimeSpan))]
    [InlineData(nameof(TypeContainer.TestNullableTimeSpan))]
    [InlineData(nameof(TypeContainer.TestEnum))]
    [InlineData(nameof(TypeContainer.TestNullableEnum))]
    [InlineData(nameof(TypeContainer.TestGuid))]
    [InlineData(nameof(TypeContainer.TestNullableGuid))]
    [InlineData(nameof(TypeContainer.TestUri))]
    [InlineData(nameof(TypeContainer.TestNullableUri))]
    [InlineData(nameof(TypeContainer.TestIPAddress))]
    [InlineData(nameof(TypeContainer.TestNullableIPAddress))]
    [InlineData(nameof(TypeContainer.TestIPNetwork))]
    [InlineData(nameof(TypeContainer.TestNullableIPNetwork))]
    [InlineData(nameof(TypeContainer.TestVersion))]
    [InlineData(nameof(TypeContainer.TestNullableVersion))]
    public async Task Can_filter_equality_with_valid_value(string propertyName)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TypeContainer>();
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        string filterValue = GetFilterValue(existingContainer, propertyName);

        string route = $"/typeContainers?filter=equals({propertyName.Camelize()},'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(existingContainer.StringId);
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestNullableBoolean))]
    [InlineData(nameof(TypeContainer.TestNullableByte))]
    [InlineData(nameof(TypeContainer.TestNullableSignedByte))]
    [InlineData(nameof(TypeContainer.TestNullableInt16))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt16))]
    [InlineData(nameof(TypeContainer.TestNullableInt32))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt32))]
    [InlineData(nameof(TypeContainer.TestNullableInt64))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt64))]
    [InlineData(nameof(TypeContainer.TestNullableInt128))]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt128))]
    [InlineData(nameof(TypeContainer.TestNullableBigInteger))]
    [InlineData(nameof(TypeContainer.TestNullableHalf))]
    [InlineData(nameof(TypeContainer.TestNullableFloat))]
    [InlineData(nameof(TypeContainer.TestNullableDouble))]
    [InlineData(nameof(TypeContainer.TestNullableDecimal))]
    [InlineData(nameof(TypeContainer.TestNullableComplex))]
    [InlineData(nameof(TypeContainer.TestNullableChar))]
    [InlineData(nameof(TypeContainer.TestNullableString))]
    [InlineData(nameof(TypeContainer.TestNullableRune))]
    [InlineData(nameof(TypeContainer.TestNullableDateTimeOffset))]
    [InlineData(nameof(TypeContainer.TestNullableDateTime))]
    [InlineData(nameof(TypeContainer.TestNullableDateOnly))]
    [InlineData(nameof(TypeContainer.TestNullableTimeOnly))]
    [InlineData(nameof(TypeContainer.TestNullableTimeSpan))]
    [InlineData(nameof(TypeContainer.TestNullableEnum))]
    [InlineData(nameof(TypeContainer.TestNullableGuid))]
    [InlineData(nameof(TypeContainer.TestNullableUri))]
    [InlineData(nameof(TypeContainer.TestNullableIPAddress))]
    [InlineData(nameof(TypeContainer.TestNullableIPNetwork))]
    [InlineData(nameof(TypeContainer.TestNullableVersion))]
    public async Task Can_filter_equality_with_null_value(string propertyName)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();
        SetResourcePropertyValueToNull(existingContainer, propertyName);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TypeContainer>();
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/typeContainers?filter=equals({propertyName.Camelize()},null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(existingContainer.StringId);
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestBoolean))]
    [InlineData(nameof(TypeContainer.TestByte))]
    [InlineData(nameof(TypeContainer.TestSignedByte))]
    [InlineData(nameof(TypeContainer.TestInt16))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt16))]
    [InlineData(nameof(TypeContainer.TestInt32))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt32))]
    [InlineData(nameof(TypeContainer.TestInt64))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt64))]
    [InlineData(nameof(TypeContainer.TestInt128))]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128))]
    [InlineData(nameof(TypeContainer.TestBigInteger))]
    [InlineData(nameof(TypeContainer.TestHalf))]
    [InlineData(nameof(TypeContainer.TestFloat))]
    [InlineData(nameof(TypeContainer.TestDouble))]
    [InlineData(nameof(TypeContainer.TestDecimal))]
    [InlineData(nameof(TypeContainer.TestComplex))]
    [InlineData(nameof(TypeContainer.TestChar))]
    [InlineData(nameof(TypeContainer.TestRune))]
    [InlineData(nameof(TypeContainer.TestDateTimeOffset))]
    [InlineData(nameof(TypeContainer.TestDateTime))]
    [InlineData(nameof(TypeContainer.TestDateOnly))]
    [InlineData(nameof(TypeContainer.TestTimeOnly))]
    [InlineData(nameof(TypeContainer.TestTimeSpan))]
    [InlineData(nameof(TypeContainer.TestEnum))]
    [InlineData(nameof(TypeContainer.TestGuid))]
    [InlineData(nameof(TypeContainer.TestIPNetwork))]
    public async Task Cannot_filter_equality_with_null_value(string propertyName)
    {
        // Arrange
        string route = $"/typeContainers?filter=equals({propertyName.Camelize()},null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().StartWith("Function, field name or value between quotes expected. Failed at position");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestBoolean), "String 'invalid' was not recognized as a valid Boolean.")]
    [InlineData(nameof(TypeContainer.TestNullableBoolean), "String 'invalid' was not recognized as a valid Boolean.")]
    [InlineData(nameof(TypeContainer.TestByte), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableByte), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestSignedByte), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableSignedByte), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestInt16), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableInt16), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt16), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt16), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestInt32), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableInt32), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt32), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt32), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestInt64), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableInt64), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt64), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt64), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestNullableBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestHalf), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableHalf), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestFloat), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableFloat), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestDouble), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableDouble), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestDecimal), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableDecimal), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestNullableComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestChar), "String must be exactly one character long.")]
    [InlineData(nameof(TypeContainer.TestNullableChar), "String must be exactly one character long.")]
    [InlineData(nameof(TypeContainer.TestDateTimeOffset), "The string 'invalid' was not recognized as a valid DateTime.")]
    [InlineData(nameof(TypeContainer.TestNullableDateTimeOffset), "The string 'invalid' was not recognized as a valid DateTime.")]
    [InlineData(nameof(TypeContainer.TestDateTime), "The string 'invalid' was not recognized as a valid DateTime.")]
    [InlineData(nameof(TypeContainer.TestNullableDateTime), "The string 'invalid' was not recognized as a valid DateTime.")]
    [InlineData(nameof(TypeContainer.TestDateOnly), "String 'invalid' was not recognized as a valid DateOnly.")]
    [InlineData(nameof(TypeContainer.TestNullableDateOnly), "String 'invalid' was not recognized as a valid DateOnly.")]
    [InlineData(nameof(TypeContainer.TestTimeOnly), "String 'invalid' was not recognized as a valid TimeOnly.")]
    [InlineData(nameof(TypeContainer.TestNullableTimeOnly), "String 'invalid' was not recognized as a valid TimeOnly.")]
    [InlineData(nameof(TypeContainer.TestTimeSpan), "String 'invalid' was not recognized as a valid TimeSpan.")]
    [InlineData(nameof(TypeContainer.TestNullableTimeSpan), "String 'invalid' was not recognized as a valid TimeSpan.")]
    [InlineData(nameof(TypeContainer.TestEnum), "Requested value 'invalid' was not found.")]
    [InlineData(nameof(TypeContainer.TestNullableEnum), "Requested value 'invalid' was not found.")]
    [InlineData(nameof(TypeContainer.TestGuid), "Unrecognized Guid format.")]
    [InlineData(nameof(TypeContainer.TestNullableGuid), "Unrecognized Guid format.")]
    [InlineData(nameof(TypeContainer.TestUri), "Invalid URI: The format of the URI could not be determined.")]
    [InlineData(nameof(TypeContainer.TestNullableUri), "Invalid URI: The format of the URI could not be determined.")]
    [InlineData(nameof(TypeContainer.TestIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestVersion), "Version string portion was too short or too long.")]
    [InlineData(nameof(TypeContainer.TestNullableVersion), "Version string portion was too short or too long.")]
    public async Task Cannot_filter_equality_with_invalid_value(string propertyName, string innerParseError)
    {
        // Arrange
        string route = $"/typeContainers?filter=equals({propertyName.Camelize()},'invalid')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().StartWith($"{GetExpectedQueryStringErrorMessage(propertyName, "invalid")} Failed at position");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
        error.Meta.Should().HaveInStackTrace($"*{innerParseError}*");
    }

    private static void SetResourcePropertyValueToNull(TypeContainer container, string propertyName)
    {
        PropertyInfo? property = typeof(TypeContainer).GetProperty(propertyName);

        if (property?.SetMethod == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        object? typedValue = RuntimeTypeConverter.GetDefaultValue(property.PropertyType);
        property.SetMethod.Invoke(container, [typedValue]);
    }

    private static string GetFilterValue(TypeContainer container, string propertyName)
    {
        PropertyInfo? property = typeof(TypeContainer).GetProperty(propertyName);

        if (property?.GetMethod == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        object? typedValue = property.GetMethod.Invoke(container, []);

        if (typedValue == null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' is null.");
        }

        Func<object, string>? converter = TypeConverterRegistry.Instance.FindToStringConverter(property.PropertyType);
        string stringValue = converter != null ? converter(typedValue) : (string)RuntimeTypeConverter.ConvertType(typedValue, typeof(string))!;

        return Uri.EscapeDataString(stringValue);
    }

    private static string GetExpectedQueryStringErrorMessage(string propertyName, string actualValue)
    {
        PropertyInfo? property = typeof(TypeContainer).GetProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        string propertyType = RuntimeTypeConverter.GetFriendlyTypeName(property.PropertyType);
        return $"Failed to convert '{actualValue}' of type 'String' to type '{propertyType}'.";
    }
}
