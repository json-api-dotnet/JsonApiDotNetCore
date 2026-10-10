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

public sealed class AttributeTypeWriteTests : IClassFixture<IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    private const string InvalidAttributeValue = "https://***";
    private const string ErrorIncorrectFormat = $"The input string '{InvalidAttributeValue}' was not in a correct format.";
    private const string ErrorPrefixJsonConvert = "The JSON value could not be converted to ";
    private const string ErrorPrefixJsonSupport = "The JSON value is not in a supported ";

    private readonly IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;
    private readonly AttributeTypesFakers _fakers = new();

    public AttributeTypeWriteTests(IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TypeContainersController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Fact]
    public async Task Can_create_resource_with_all_attributes_set_to_valid_values()
    {
        // Arrange
        TypeContainer newContainer = _fakers.TypeContainer.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "typeContainers",
                attributes = new
                {
                    testBoolean = newContainer.TestBoolean,
                    testNullableBoolean = newContainer.TestNullableBoolean,
                    testByte = newContainer.TestByte,
                    testNullableByte = newContainer.TestNullableByte,
                    testSignedByte = newContainer.TestSignedByte,
                    testNullableSignedByte = newContainer.TestNullableSignedByte,
                    testInt16 = newContainer.TestInt16,
                    testNullableInt16 = newContainer.TestNullableInt16,
                    testUnsignedInt16 = newContainer.TestUnsignedInt16,
                    testNullableUnsignedInt16 = newContainer.TestNullableUnsignedInt16,
                    testInt32 = newContainer.TestInt32,
                    testNullableInt32 = newContainer.TestNullableInt32,
                    testUnsignedInt32 = newContainer.TestUnsignedInt32,
                    testNullableUnsignedInt32 = newContainer.TestNullableUnsignedInt32,
                    testInt64 = newContainer.TestInt64,
                    testNullableInt64 = newContainer.TestNullableInt64,
                    testUnsignedInt64 = newContainer.TestUnsignedInt64,
                    testNullableUnsignedInt64 = newContainer.TestNullableUnsignedInt64,
                    testInt128 = newContainer.TestInt128,
                    testNullableInt128 = newContainer.TestNullableInt128,
                    testUnsignedInt128 = newContainer.TestUnsignedInt128,
                    testNullableUnsignedInt128 = newContainer.TestNullableUnsignedInt128,
                    testBigInteger = newContainer.TestBigInteger,
                    testNullableBigInteger = newContainer.TestNullableBigInteger,
                    testHalf = newContainer.TestHalf,
                    testNullableHalf = newContainer.TestNullableHalf,
                    testFloat = newContainer.TestFloat,
                    testNullableFloat = newContainer.TestNullableFloat,
                    testDouble = newContainer.TestDouble,
                    testNullableDouble = newContainer.TestNullableDouble,
                    testDecimal = newContainer.TestDecimal,
                    testNullableDecimal = newContainer.TestNullableDecimal,
                    testComplex = newContainer.TestComplex,
                    testNullableComplex = newContainer.TestNullableComplex,
                    testChar = newContainer.TestChar,
                    testNullableChar = newContainer.TestNullableChar,
                    testString = newContainer.TestString,
                    testNullableString = newContainer.TestNullableString,
                    testRune = newContainer.TestRune,
                    testNullableRune = newContainer.TestNullableRune,
                    testDateTimeOffset = newContainer.TestDateTimeOffset,
                    testNullableDateTimeOffset = newContainer.TestNullableDateTimeOffset,
                    testDateTime = newContainer.TestDateTime,
                    testNullableDateTime = newContainer.TestNullableDateTime,
                    testDateTimeStoredInLocalTimeZone = newContainer.TestDateTimeStoredInLocalTimeZone,
                    testDateOnly = newContainer.TestDateOnly,
                    testNullableDateOnly = newContainer.TestNullableDateOnly,
                    testTimeOnly = newContainer.TestTimeOnly,
                    testNullableTimeOnly = newContainer.TestNullableTimeOnly,
                    testTimeSpan = newContainer.TestTimeSpan,
                    testNullableTimeSpan = newContainer.TestNullableTimeSpan,
                    testEnum = newContainer.TestEnum,
                    testNullableEnum = newContainer.TestNullableEnum,
                    testGuid = newContainer.TestGuid,
                    testNullableGuid = newContainer.TestNullableGuid,
                    testUri = newContainer.TestUri,
                    testNullableUri = newContainer.TestNullableUri,
                    testIPAddress = newContainer.TestIPAddress,
                    testNullableIPAddress = newContainer.TestNullableIPAddress,
                    testIPNetwork = newContainer.TestIPNetwork,
                    testNullableIPNetwork = newContainer.TestNullableIPNetwork,
                    testVersion = newContainer.TestVersion,
                    testNullableVersion = newContainer.TestNullableVersion
                }
            }
        };

        const string route = "/typeContainers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("testBoolean").WhoseValue.Should().Be(newContainer.TestBoolean);
            resource.Attributes.Should().ContainKey("testNullableBoolean").WhoseValue.Should().Be(newContainer.TestNullableBoolean);
            resource.Attributes.Should().ContainKey("testByte").WhoseValue.Should().Be(newContainer.TestByte);
            resource.Attributes.Should().ContainKey("testNullableByte").WhoseValue.Should().Be(newContainer.TestNullableByte);
            resource.Attributes.Should().ContainKey("testSignedByte").WhoseValue.Should().Be(newContainer.TestSignedByte);
            resource.Attributes.Should().ContainKey("testNullableSignedByte").WhoseValue.Should().Be(newContainer.TestNullableSignedByte);
            resource.Attributes.Should().ContainKey("testInt16").WhoseValue.Should().Be(newContainer.TestInt16);
            resource.Attributes.Should().ContainKey("testNullableInt16").WhoseValue.Should().Be(newContainer.TestNullableInt16);
            resource.Attributes.Should().ContainKey("testUnsignedInt16").WhoseValue.Should().Be(newContainer.TestUnsignedInt16);
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt16").WhoseValue.Should().Be(newContainer.TestNullableUnsignedInt16);
            resource.Attributes.Should().ContainKey("testInt32").WhoseValue.Should().Be(newContainer.TestInt32);
            resource.Attributes.Should().ContainKey("testNullableInt32").WhoseValue.Should().Be(newContainer.TestNullableInt32);
            resource.Attributes.Should().ContainKey("testUnsignedInt32").WhoseValue.Should().Be(newContainer.TestUnsignedInt32);
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt32").WhoseValue.Should().Be(newContainer.TestNullableUnsignedInt32);
            resource.Attributes.Should().ContainKey("testInt64").WhoseValue.Should().Be(newContainer.TestInt64);
            resource.Attributes.Should().ContainKey("testNullableInt64").WhoseValue.Should().Be(newContainer.TestNullableInt64);
            resource.Attributes.Should().ContainKey("testUnsignedInt64").WhoseValue.Should().Be(newContainer.TestUnsignedInt64);
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt64").WhoseValue.Should().Be(newContainer.TestNullableUnsignedInt64);
            resource.Attributes.Should().ContainKey("testInt128").WhoseValue.Should().Be(newContainer.TestInt128);
            resource.Attributes.Should().ContainKey("testNullableInt128").WhoseValue.Should().Be(newContainer.TestNullableInt128);
            resource.Attributes.Should().ContainKey("testUnsignedInt128").WhoseValue.Should().Be(newContainer.TestUnsignedInt128);
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt128").WhoseValue.Should().Be(newContainer.TestNullableUnsignedInt128);
            resource.Attributes.Should().ContainKey("testBigInteger").WhoseValue.Should().Be(newContainer.TestBigInteger);
            resource.Attributes.Should().ContainKey("testNullableBigInteger").WhoseValue.Should().Be(newContainer.TestNullableBigInteger);
            resource.Attributes.Should().ContainKey("testHalf").WhoseValue.Should().Be(newContainer.TestHalf);
            resource.Attributes.Should().ContainKey("testNullableHalf").WhoseValue.Should().Be(newContainer.TestNullableHalf);
            resource.Attributes.Should().ContainKey("testFloat").WhoseValue.Should().Be(newContainer.TestFloat);
            resource.Attributes.Should().ContainKey("testNullableFloat").WhoseValue.Should().Be(newContainer.TestNullableFloat);
            resource.Attributes.Should().ContainKey("testDouble").WhoseValue.Should().Be(newContainer.TestDouble);
            resource.Attributes.Should().ContainKey("testNullableDouble").WhoseValue.Should().Be(newContainer.TestNullableDouble);
            resource.Attributes.Should().ContainKey("testDecimal").WhoseValue.Should().Be(newContainer.TestDecimal);
            resource.Attributes.Should().ContainKey("testNullableDecimal").WhoseValue.Should().Be(newContainer.TestNullableDecimal);
            resource.Attributes.Should().ContainKey("testComplex").WhoseValue.Should().Be(newContainer.TestComplex);
            resource.Attributes.Should().ContainKey("testNullableComplex").WhoseValue.Should().Be(newContainer.TestNullableComplex);
            resource.Attributes.Should().ContainKey("testChar").WhoseValue.Should().Be(newContainer.TestChar);
            resource.Attributes.Should().ContainKey("testNullableChar").WhoseValue.Should().Be(newContainer.TestNullableChar);
            resource.Attributes.Should().ContainKey("testString").WhoseValue.Should().Be(newContainer.TestString);
            resource.Attributes.Should().ContainKey("testNullableString").WhoseValue.Should().Be(newContainer.TestNullableString);
            resource.Attributes.Should().ContainKey("testRune").WhoseValue.Should().Be(newContainer.TestRune);
            resource.Attributes.Should().ContainKey("testNullableRune").WhoseValue.Should().Be(newContainer.TestNullableRune);
            resource.Attributes.Should().ContainKey("testDateTimeOffset").WhoseValue.Should().Be(newContainer.TestDateTimeOffset);
            resource.Attributes.Should().ContainKey("testNullableDateTimeOffset").WhoseValue.Should().Be(newContainer.TestNullableDateTimeOffset);
            resource.Attributes.Should().ContainKey("testDateTime").WhoseValue.Should().Be(newContainer.TestDateTime);
            resource.Attributes.Should().ContainKey("testNullableDateTime").WhoseValue.Should().Be(newContainer.TestNullableDateTime);
            resource.Attributes.Should().ContainKey("testDateTimeStoredInLocalTimeZone").WhoseValue.Should().Be(newContainer.TestDateTimeStoredInLocalTimeZone);
            resource.Attributes.Should().ContainKey("testDateOnly").WhoseValue.Should().Be(newContainer.TestDateOnly);
            resource.Attributes.Should().ContainKey("testNullableDateOnly").WhoseValue.Should().Be(newContainer.TestNullableDateOnly);
            resource.Attributes.Should().ContainKey("testTimeOnly").WhoseValue.Should().Be(newContainer.TestTimeOnly);
            resource.Attributes.Should().ContainKey("testNullableTimeOnly").WhoseValue.Should().Be(newContainer.TestNullableTimeOnly);
            resource.Attributes.Should().ContainKey("testTimeSpan").WhoseValue.Should().Be(newContainer.TestTimeSpan);
            resource.Attributes.Should().ContainKey("testNullableTimeSpan").WhoseValue.Should().Be(newContainer.TestNullableTimeSpan);
            resource.Attributes.Should().ContainKey("testEnum").WhoseValue.Should().Be(newContainer.TestEnum);
            resource.Attributes.Should().ContainKey("testNullableEnum").WhoseValue.Should().Be(newContainer.TestNullableEnum);
            resource.Attributes.Should().ContainKey("testGuid").WhoseValue.Should().Be(newContainer.TestGuid);
            resource.Attributes.Should().ContainKey("testNullableGuid").WhoseValue.Should().Be(newContainer.TestNullableGuid);
            resource.Attributes.Should().ContainKey("testUri").WhoseValue.Should().Be(newContainer.TestUri);
            resource.Attributes.Should().ContainKey("testNullableUri").WhoseValue.Should().Be(newContainer.TestNullableUri);
            resource.Attributes.Should().ContainKey("testIPAddress").WhoseValue.Should().Be(newContainer.TestIPAddress);
            resource.Attributes.Should().ContainKey("testNullableIPAddress").WhoseValue.Should().Be(newContainer.TestNullableIPAddress);
            resource.Attributes.Should().ContainKey("testIPNetwork").WhoseValue.Should().Be(newContainer.TestIPNetwork);
            resource.Attributes.Should().ContainKey("testNullableIPNetwork").WhoseValue.Should().Be(newContainer.TestNullableIPNetwork);
            resource.Attributes.Should().ContainKey("testVersion").WhoseValue.Should().Be(newContainer.TestVersion);
            resource.Attributes.Should().ContainKey("testNullableVersion").WhoseValue.Should().Be(newContainer.TestNullableVersion);
        });

        long newContainerId = long.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TypeContainer containerInDatabase = await dbContext.TypeContainers.FirstWithIdAsync(newContainerId);

            containerInDatabase.TestBoolean.Should().Be(newContainer.TestBoolean);
            containerInDatabase.TestNullableBoolean.Should().Be(newContainer.TestNullableBoolean);
            containerInDatabase.TestByte.Should().Be(newContainer.TestByte);
            containerInDatabase.TestNullableByte.Should().Be(newContainer.TestNullableByte);
            containerInDatabase.TestSignedByte.Should().Be(newContainer.TestSignedByte);
            containerInDatabase.TestNullableSignedByte.Should().Be(newContainer.TestNullableSignedByte);
            containerInDatabase.TestInt16.Should().Be(newContainer.TestInt16);
            containerInDatabase.TestNullableInt16.Should().Be(newContainer.TestNullableInt16);
            containerInDatabase.TestUnsignedInt16.Should().Be(newContainer.TestUnsignedInt16);
            containerInDatabase.TestNullableUnsignedInt16.Should().Be(newContainer.TestNullableUnsignedInt16);
            containerInDatabase.TestInt32.Should().Be(newContainer.TestInt32);
            containerInDatabase.TestNullableInt32.Should().Be(newContainer.TestNullableInt32);
            containerInDatabase.TestUnsignedInt32.Should().Be(newContainer.TestUnsignedInt32);
            containerInDatabase.TestNullableUnsignedInt32.Should().Be(newContainer.TestNullableUnsignedInt32);
            containerInDatabase.TestInt64.Should().Be(newContainer.TestInt64);
            containerInDatabase.TestNullableInt64.Should().Be(newContainer.TestNullableInt64);
            containerInDatabase.TestUnsignedInt64.Should().Be(newContainer.TestUnsignedInt64);
            containerInDatabase.TestNullableUnsignedInt64.Should().Be(newContainer.TestNullableUnsignedInt64);
            containerInDatabase.TestInt128.Should().Be(newContainer.TestInt128);
            containerInDatabase.TestNullableInt128.Should().Be(newContainer.TestNullableInt128);
            containerInDatabase.TestUnsignedInt128.Should().Be(newContainer.TestUnsignedInt128);
            containerInDatabase.TestNullableUnsignedInt128.Should().Be(newContainer.TestNullableUnsignedInt128);
            containerInDatabase.TestBigInteger.Should().Be(newContainer.TestBigInteger);
            containerInDatabase.TestNullableBigInteger.Should().Be(newContainer.TestNullableBigInteger);
            containerInDatabase.TestHalf.Should().Be(newContainer.TestHalf);
            containerInDatabase.TestNullableHalf.Should().Be(newContainer.TestNullableHalf);
            containerInDatabase.TestFloat.Should().Be(newContainer.TestFloat);
            containerInDatabase.TestNullableFloat.Should().Be(newContainer.TestNullableFloat);
            containerInDatabase.TestDouble.Should().Be(newContainer.TestDouble);
            containerInDatabase.TestNullableDouble.Should().Be(newContainer.TestNullableDouble);
            containerInDatabase.TestDecimal.Should().Be(newContainer.TestDecimal);
            containerInDatabase.TestNullableDecimal.Should().Be(newContainer.TestNullableDecimal);
            containerInDatabase.TestComplex.Should().Be(newContainer.TestComplex);
            containerInDatabase.TestNullableComplex.Should().Be(newContainer.TestNullableComplex);
            containerInDatabase.TestChar.Should().Be(newContainer.TestChar);
            containerInDatabase.TestNullableChar.Should().Be(newContainer.TestNullableChar);
            containerInDatabase.TestString.Should().Be(newContainer.TestString);
            containerInDatabase.TestNullableString.Should().Be(newContainer.TestNullableString);
            containerInDatabase.TestRune.Should().Be(newContainer.TestRune);
            containerInDatabase.TestNullableRune.Should().Be(newContainer.TestNullableRune);
            containerInDatabase.TestDateTimeOffset.Should().Be(newContainer.TestDateTimeOffset);
            containerInDatabase.TestNullableDateTimeOffset.Should().Be(newContainer.TestNullableDateTimeOffset);
            containerInDatabase.TestDateTime.Should().Be(newContainer.TestDateTime);
            containerInDatabase.TestNullableDateTime.Should().Be(newContainer.TestNullableDateTime);
            containerInDatabase.TestDateTimeStoredInLocalTimeZone.Should().Be(newContainer.TestDateTimeStoredInLocalTimeZone);
            containerInDatabase.TestDateOnly.Should().Be(newContainer.TestDateOnly);
            containerInDatabase.TestNullableDateOnly.Should().Be(newContainer.TestNullableDateOnly);
            containerInDatabase.TestTimeOnly.Should().Be(newContainer.TestTimeOnly);
            containerInDatabase.TestNullableTimeOnly.Should().Be(newContainer.TestNullableTimeOnly);
            containerInDatabase.TestTimeSpan.Should().Be(newContainer.TestTimeSpan);
            containerInDatabase.TestNullableTimeSpan.Should().Be(newContainer.TestNullableTimeSpan);
            containerInDatabase.TestEnum.Should().Be(newContainer.TestEnum);
            containerInDatabase.TestNullableEnum.Should().Be(newContainer.TestNullableEnum);
            containerInDatabase.TestGuid.Should().Be(newContainer.TestGuid);
            containerInDatabase.TestNullableGuid.Should().Be(newContainer.TestNullableGuid);
            containerInDatabase.TestUri.Should().Be(newContainer.TestUri);
            containerInDatabase.TestNullableUri.Should().Be(newContainer.TestNullableUri);
            containerInDatabase.TestIPAddress.Should().Be(newContainer.TestIPAddress);
            containerInDatabase.TestNullableIPAddress.Should().Be(newContainer.TestNullableIPAddress);
            containerInDatabase.TestIPNetwork.Should().Be(newContainer.TestIPNetwork);
            containerInDatabase.TestNullableIPNetwork.Should().Be(newContainer.TestNullableIPNetwork);
            containerInDatabase.TestVersion.Should().Be(newContainer.TestVersion);
            containerInDatabase.TestNullableVersion.Should().Be(newContainer.TestNullableVersion);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_nullable_attributes_set_to_null()
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "typeContainers",
                id = existingContainer.StringId,
                attributes = new
                {
                    testNullableBoolean = (object?)null,
                    testNullableByte = (object?)null,
                    testNullableSignedByte = (object?)null,
                    testNullableInt16 = (object?)null,
                    testNullableUnsignedInt16 = (object?)null,
                    testNullableInt32 = (object?)null,
                    testNullableUnsignedInt32 = (object?)null,
                    testNullableInt64 = (object?)null,
                    testNullableUnsignedInt64 = (object?)null,
                    testNullableInt128 = (object?)null,
                    testNullableUnsignedInt128 = (object?)null,
                    testNullableBigInteger = (object?)null,
                    testNullableHalf = (object?)null,
                    testNullableFloat = (object?)null,
                    testNullableDouble = (object?)null,
                    testNullableDecimal = (object?)null,
                    testNullableComplex = (object?)null,
                    testNullableChar = (object?)null,
                    testNullableString = (object?)null,
                    testNullableRune = (object?)null,
                    testNullableDateTimeOffset = (object?)null,
                    testNullableDateTime = (object?)null,
                    testNullableDateOnly = (object?)null,
                    testNullableTimeOnly = (object?)null,
                    testNullableTimeSpan = (object?)null,
                    testNullableEnum = (object?)null,
                    testNullableGuid = (object?)null,
                    testNullableUri = (object?)null,
                    testNullableIPAddress = (object?)null,
                    testNullableIPNetwork = (object?)null,
                    testNullableVersion = (object?)null
                }
            }
        };

        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("testNullableBoolean").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableByte").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableSignedByte").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableInt16").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt16").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableInt32").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt32").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableInt64").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt64").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableInt128").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableUnsignedInt128").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableBigInteger").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableHalf").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableFloat").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableDouble").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableDecimal").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableComplex").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableChar").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableString").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableRune").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableDateTimeOffset").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableDateTime").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableDateOnly").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableTimeOnly").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableTimeSpan").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableEnum").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableGuid").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableUri").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableIPAddress").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableIPNetwork").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("testNullableVersion").WhoseValue.Should().BeNull();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TypeContainer containerInDatabase = await dbContext.TypeContainers.FirstWithIdAsync(existingContainer.Id);

            containerInDatabase.TestNullableBoolean.Should().BeNull();
            containerInDatabase.TestNullableByte.Should().BeNull();
            containerInDatabase.TestNullableSignedByte.Should().BeNull();
            containerInDatabase.TestNullableInt16.Should().BeNull();
            containerInDatabase.TestNullableUnsignedInt16.Should().BeNull();
            containerInDatabase.TestNullableInt32.Should().BeNull();
            containerInDatabase.TestNullableUnsignedInt32.Should().BeNull();
            containerInDatabase.TestNullableInt64.Should().BeNull();
            containerInDatabase.TestNullableUnsignedInt64.Should().BeNull();
            containerInDatabase.TestNullableInt128.Should().BeNull();
            containerInDatabase.TestNullableUnsignedInt128.Should().BeNull();
            containerInDatabase.TestNullableBigInteger.Should().BeNull();
            containerInDatabase.TestNullableHalf.Should().BeNull();
            containerInDatabase.TestNullableFloat.Should().BeNull();
            containerInDatabase.TestNullableDouble.Should().BeNull();
            containerInDatabase.TestNullableDecimal.Should().BeNull();
            containerInDatabase.TestNullableComplex.Should().BeNull();
            containerInDatabase.TestNullableChar.Should().BeNull();
            containerInDatabase.TestNullableString.Should().BeNull();
            containerInDatabase.TestNullableRune.Should().BeNull();
            containerInDatabase.TestNullableDateTimeOffset.Should().BeNull();
            containerInDatabase.TestNullableDateTime.Should().BeNull();
            containerInDatabase.TestNullableDateOnly.Should().BeNull();
            containerInDatabase.TestNullableTimeOnly.Should().BeNull();
            containerInDatabase.TestNullableTimeSpan.Should().BeNull();
            containerInDatabase.TestNullableEnum.Should().BeNull();
            containerInDatabase.TestNullableGuid.Should().BeNull();
            containerInDatabase.TestNullableUri.Should().BeNull();
            containerInDatabase.TestNullableIPAddress.Should().BeNull();
            containerInDatabase.TestNullableIPNetwork.Should().BeNull();
            containerInDatabase.TestNullableVersion.Should().BeNull();
        });
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestBoolean), false)]
    [InlineData(nameof(TypeContainer.TestByte), false)]
    [InlineData(nameof(TypeContainer.TestSignedByte), false)]
    [InlineData(nameof(TypeContainer.TestInt16), false)]
    [InlineData(nameof(TypeContainer.TestUnsignedInt16), false)]
    [InlineData(nameof(TypeContainer.TestInt32), false)]
    [InlineData(nameof(TypeContainer.TestUnsignedInt32), false)]
    [InlineData(nameof(TypeContainer.TestInt64), false)]
    [InlineData(nameof(TypeContainer.TestUnsignedInt64), false)]
    [InlineData(nameof(TypeContainer.TestInt128), false)]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128), false)]
    [InlineData(nameof(TypeContainer.TestBigInteger), false)]
    [InlineData(nameof(TypeContainer.TestHalf), false)]
    [InlineData(nameof(TypeContainer.TestFloat), false)]
    [InlineData(nameof(TypeContainer.TestDouble), false)]
    [InlineData(nameof(TypeContainer.TestDecimal), false)]
    [InlineData(nameof(TypeContainer.TestComplex), false)]
    [InlineData(nameof(TypeContainer.TestChar), false)]
    [InlineData(nameof(TypeContainer.TestString), true)]
    [InlineData(nameof(TypeContainer.TestRune), false)]
    [InlineData(nameof(TypeContainer.TestDateTimeOffset), false)]
    [InlineData(nameof(TypeContainer.TestDateTime), false)]
    [InlineData(nameof(TypeContainer.TestDateOnly), false)]
    [InlineData(nameof(TypeContainer.TestTimeOnly), false)]
    [InlineData(nameof(TypeContainer.TestTimeSpan), false)]
    [InlineData(nameof(TypeContainer.TestEnum), false)]
    [InlineData(nameof(TypeContainer.TestGuid), false)]
    [InlineData(nameof(TypeContainer.TestUri), true)]
    [InlineData(nameof(TypeContainer.TestIPAddress), true)]
    [InlineData(nameof(TypeContainer.TestIPNetwork), false)]
    [InlineData(nameof(TypeContainer.TestVersion), true)]
    public async Task Cannot_update_resource_with_attribute_set_to_null(string propertyName, bool failAtModelValidation)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "typeContainers",
                id = existingContainer.StringId,
                attributes = new Dictionary<string, object?>()
            }
        };

        SetAttributeValueInUpdateRequest(requestBody.data.attributes, propertyName, null);

        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        if (failAtModelValidation)
        {
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be($"The {propertyName} field is required.");
        }
        else
        {
            error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
            error.Detail.Should().Be(GetExpectedConverterErrorMessage(propertyName, null));
        }

        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be($"/data/attributes/{propertyName.Camelize()}");
    }

    [Theory]
    [InlineData(nameof(TypeContainer.TestBoolean), $"{ErrorPrefixJsonConvert}System.Boolean.")]
    [InlineData(nameof(TypeContainer.TestNullableBoolean), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Boolean].")]
    [InlineData(nameof(TypeContainer.TestByte), $"{ErrorPrefixJsonConvert}System.Byte.")]
    [InlineData(nameof(TypeContainer.TestNullableByte), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Byte].")]
    [InlineData(nameof(TypeContainer.TestSignedByte), $"{ErrorPrefixJsonConvert}System.SByte.")]
    [InlineData(nameof(TypeContainer.TestNullableSignedByte), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.SByte].")]
    [InlineData(nameof(TypeContainer.TestInt16), $"{ErrorPrefixJsonConvert}System.Int16.")]
    [InlineData(nameof(TypeContainer.TestNullableInt16), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Int16].")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt16), $"{ErrorPrefixJsonConvert}System.UInt16.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt16), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.UInt16].")]
    [InlineData(nameof(TypeContainer.TestInt32), $"{ErrorPrefixJsonConvert}System.Int32.")]
    [InlineData(nameof(TypeContainer.TestNullableInt32), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Int32].")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt32), $"{ErrorPrefixJsonConvert}System.UInt32.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt32), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.UInt32].")]
    [InlineData(nameof(TypeContainer.TestInt64), $"{ErrorPrefixJsonConvert}System.Int64.")]
    [InlineData(nameof(TypeContainer.TestNullableInt64), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Int64].")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt64), $"{ErrorPrefixJsonConvert}System.UInt64.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt64), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.UInt64].")]
    [InlineData(nameof(TypeContainer.TestInt128), ErrorIncorrectFormat)]
    [InlineData(nameof(TypeContainer.TestNullableInt128), ErrorIncorrectFormat)]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128), ErrorIncorrectFormat)]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt128), ErrorIncorrectFormat)]
    [InlineData(nameof(TypeContainer.TestBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestNullableBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestHalf), $"{ErrorPrefixJsonConvert}System.Half.")]
    [InlineData(nameof(TypeContainer.TestNullableHalf), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Half].")]
    [InlineData(nameof(TypeContainer.TestFloat), $"{ErrorPrefixJsonConvert}System.Single.")]
    [InlineData(nameof(TypeContainer.TestNullableFloat), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Single].")]
    [InlineData(nameof(TypeContainer.TestDouble), $"{ErrorPrefixJsonConvert}System.Double.")]
    [InlineData(nameof(TypeContainer.TestNullableDouble), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Double].")]
    [InlineData(nameof(TypeContainer.TestDecimal), $"{ErrorPrefixJsonConvert}System.Decimal.")]
    [InlineData(nameof(TypeContainer.TestNullableDecimal), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Decimal].")]
    [InlineData(nameof(TypeContainer.TestComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestNullableComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestChar), $"{ErrorPrefixJsonConvert}System.Char.")]
    [InlineData(nameof(TypeContainer.TestNullableChar), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.Char].")]
    [InlineData(nameof(TypeContainer.TestDateTimeOffset), $"{ErrorPrefixJsonSupport}DateTimeOffset format.")]
    [InlineData(nameof(TypeContainer.TestNullableDateTimeOffset), $"{ErrorPrefixJsonSupport}DateTimeOffset format.")]
    [InlineData(nameof(TypeContainer.TestDateTime), $"{ErrorPrefixJsonSupport}DateTime format.")]
    [InlineData(nameof(TypeContainer.TestNullableDateTime), $"{ErrorPrefixJsonSupport}DateTime format.")]
    [InlineData(nameof(TypeContainer.TestDateOnly), $"{ErrorPrefixJsonSupport}DateOnly format.")]
    [InlineData(nameof(TypeContainer.TestNullableDateOnly), $"{ErrorPrefixJsonSupport}DateOnly format.")]
    [InlineData(nameof(TypeContainer.TestTimeOnly), $"{ErrorPrefixJsonSupport}TimeOnly format.")]
    [InlineData(nameof(TypeContainer.TestNullableTimeOnly), $"{ErrorPrefixJsonSupport}TimeOnly format.")]
    [InlineData(nameof(TypeContainer.TestTimeSpan), $"{ErrorPrefixJsonSupport}TimeSpan format.")]
    [InlineData(nameof(TypeContainer.TestNullableTimeSpan), $"{ErrorPrefixJsonSupport}TimeSpan format.")]
    [InlineData(nameof(TypeContainer.TestEnum), $"{ErrorPrefixJsonConvert}System.DayOfWeek.")]
    [InlineData(nameof(TypeContainer.TestNullableEnum), $"{ErrorPrefixJsonConvert}System.Nullable`1[System.DayOfWeek].")]
    [InlineData(nameof(TypeContainer.TestGuid), $"{ErrorPrefixJsonSupport}Guid format.")]
    [InlineData(nameof(TypeContainer.TestNullableGuid), $"{ErrorPrefixJsonSupport}Guid format.")]
    [InlineData(nameof(TypeContainer.TestUri), $"{ErrorPrefixJsonConvert}System.Uri.")]
    [InlineData(nameof(TypeContainer.TestNullableUri), $"{ErrorPrefixJsonConvert}System.Uri.")]
    [InlineData(nameof(TypeContainer.TestIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestVersion), $"{ErrorPrefixJsonConvert}System.Version.")]
    [InlineData(nameof(TypeContainer.TestNullableVersion), $"{ErrorPrefixJsonConvert}System.Version.")]
    public async Task Cannot_update_resource_with_attribute_set_to_invalid_value(string propertyName, string innerParseError)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "typeContainers",
                id = existingContainer.StringId,
                attributes = new Dictionary<string, object?>()
            }
        };

        SetAttributeValueInUpdateRequest(requestBody.data.attributes, propertyName, InvalidAttributeValue);

        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Detail.Should().Be(GetExpectedConverterErrorMessage(propertyName, InvalidAttributeValue));
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be($"/data/attributes/{propertyName.Camelize()}");
        error.Meta.Should().HaveInStackTrace($"*{innerParseError}*");
    }

    private static void SetAttributeValueInUpdateRequest(Dictionary<string, object?> attributes, string propertyName, object? value)
    {
        PropertyInfo? property = typeof(TypeContainer).GetProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        string attributeName = propertyName.Camelize();
        attributes[attributeName] = value;
    }

    private static string GetExpectedConverterErrorMessage(string propertyName, string? actualValue)
    {
        PropertyInfo? property = typeof(TypeContainer).GetProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        string propertyType = RuntimeTypeConverter.GetFriendlyTypeName(property.PropertyType);
        string jsonType = actualValue == null ? "Null" : "String";
        return $"Failed to convert attribute '{propertyName.Camelize()}' with value '{actualValue}' of type '{jsonType}' to type '{propertyType}'.";
    }
}
