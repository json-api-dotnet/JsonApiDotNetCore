using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Reflection;
using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.AttributeTypes.GeneratedCode;
using OpenApiKiotaEndToEndTests.AttributeTypes.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.AttributeTypes;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using ClientDayOfWeek = OpenApiKiotaEndToEndTests.AttributeTypes.GeneratedCode.Models.DayOfWeekObject;
using IJsonApiOptions = JsonApiDotNetCore.Configuration.IJsonApiOptions;
using ServerDayOfWeek = System.DayOfWeek;

namespace OpenApiKiotaEndToEndTests.AttributeTypes;

public sealed class AttributeTypeTests : IClassFixture<IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly AttributeTypesFakers _fakers = new();

    public AttributeTypeTests(IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<TypeContainersController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));

        var options = testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.SerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
        options.SerializerOptions.Converters.Add(new TimeSpanAsXmlJsonConverter());
    }

    [Fact]
    public async Task Can_create_resource_with_all_attributes_set_to_valid_values()
    {
        // Arrange
        TypeContainer newContainer = _fakers.TypeContainer.GenerateOne();
        newContainer.TestTimeOnly = newContainer.TestTimeOnly.TruncateToWholeSeconds();
        newContainer.TestNullableTimeOnly = newContainer.TestNullableTimeOnly?.TruncateToWholeSeconds();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        var requestBody = new CreateTypeContainerRequestDocument
        {
            Data = new DataInCreateTypeContainerRequest
            {
                Type = TypeContainerResourceType.TypeContainers,
                Attributes = new AttributesInCreateTypeContainerRequest
                {
                    TestBoolean = newContainer.TestBoolean,
                    TestNullableBoolean = newContainer.TestNullableBoolean,
                    TestByte = newContainer.TestByte,
                    TestNullableByte = newContainer.TestNullableByte,
                    TestSignedByte = newContainer.TestSignedByte,
                    TestNullableSignedByte = newContainer.TestNullableSignedByte,
                    TestInt16 = newContainer.TestInt16,
                    TestNullableInt16 = newContainer.TestNullableInt16,
                    TestUnsignedInt16 = newContainer.TestUnsignedInt16,
                    TestNullableUnsignedInt16 = newContainer.TestNullableUnsignedInt16,
                    TestInt32 = newContainer.TestInt32,
                    TestNullableInt32 = newContainer.TestNullableInt32,
                    TestUnsignedInt32 = checked((int)newContainer.TestUnsignedInt32),
                    TestNullableUnsignedInt32 = checked((int?)newContainer.TestNullableUnsignedInt32),
                    TestInt64 = newContainer.TestInt64,
                    TestNullableInt64 = newContainer.TestNullableInt64,
                    TestUnsignedInt64 = checked((long)newContainer.TestUnsignedInt64),
                    TestNullableUnsignedInt64 = checked((long?)newContainer.TestNullableUnsignedInt64),
                    TestInt128 = newContainer.TestInt128.ToString(CultureInfo.InvariantCulture),
                    TestNullableInt128 = newContainer.TestNullableInt128?.ToString(CultureInfo.InvariantCulture),
                    TestUnsignedInt128 = newContainer.TestUnsignedInt128.ToString(),
                    TestNullableUnsignedInt128 = newContainer.TestNullableUnsignedInt128?.ToString(),
                    TestBigInteger = newContainer.TestBigInteger.ToString(CultureInfo.InvariantCulture),
                    TestNullableBigInteger = newContainer.TestNullableBigInteger?.ToString(CultureInfo.InvariantCulture),
                    TestHalf = newContainer.TestHalf.AsFloat(),
                    TestNullableHalf = newContainer.TestNullableHalf?.AsFloat(),
                    TestFloat = newContainer.TestFloat,
                    TestNullableFloat = newContainer.TestNullableFloat,
                    TestDouble = newContainer.TestDouble,
                    TestNullableDouble = newContainer.TestNullableDouble,
                    TestDecimal = (double)newContainer.TestDecimal,
                    TestNullableDecimal = (double?)newContainer.TestNullableDecimal,
                    TestComplex = newContainer.TestComplex.ToString(CultureInfo.InvariantCulture),
                    TestNullableComplex = newContainer.TestNullableComplex?.ToString(CultureInfo.InvariantCulture),
                    TestChar = newContainer.TestChar.ToString(CultureInfo.InvariantCulture),
                    TestNullableChar = newContainer.TestNullableChar?.ToString(CultureInfo.InvariantCulture),
                    TestString = newContainer.TestString,
                    TestNullableString = newContainer.TestNullableString,
                    TestRune = newContainer.TestRune.ToString(),
                    TestNullableRune = newContainer.TestNullableRune?.ToString(),
                    TestDateTimeOffset = newContainer.TestDateTimeOffset,
                    TestNullableDateTimeOffset = newContainer.TestNullableDateTimeOffset,
                    TestDateTime = newContainer.TestDateTime,
                    TestNullableDateTime = newContainer.TestNullableDateTime,
                    TestDateOnly = newContainer.TestDateOnly,
                    TestNullableDateOnly = newContainer.TestNullableDateOnly,
                    TestTimeOnly = newContainer.TestTimeOnly,
                    TestNullableTimeOnly = newContainer.TestNullableTimeOnly,
                    TestTimeSpan = newContainer.TestTimeSpan,
                    TestNullableTimeSpan = newContainer.TestNullableTimeSpan,
                    TestEnum = MapEnum<ServerDayOfWeek, ClientDayOfWeek>(newContainer.TestEnum),
                    TestNullableEnum = MapEnum<ServerDayOfWeek, ClientDayOfWeek>(newContainer.TestNullableEnum),
                    TestGuid = newContainer.TestGuid,
                    TestNullableGuid = newContainer.TestNullableGuid,
                    TestUri = newContainer.TestUri.ToString(),
                    TestNullableUri = newContainer.TestNullableUri?.ToString(),
                    TestIPAddress = newContainer.TestIPAddress.ToString(),
                    TestNullableIPAddress = newContainer.TestNullableIPAddress?.ToString(),
                    TestIPNetwork = newContainer.TestIPNetwork.ToString(),
                    TestNullableIPNetwork = newContainer.TestNullableIPNetwork?.ToString(),
                    TestVersion = newContainer.TestVersion.ToString(),
                    TestNullableVersion = newContainer.TestNullableVersion?.ToString()
                }
            }
        };

        // Act
        PrimaryTypeContainerResponseDocument? response = await apiClient.TypeContainers.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.TestBoolean.Should().Be(newContainer.TestBoolean);
        response.Data.Attributes.TestNullableBoolean.Should().Be(newContainer.TestNullableBoolean);
        response.Data.Attributes.TestByte.Should().Be(newContainer.TestByte);
        response.Data.Attributes.TestNullableByte.Should().Be(newContainer.TestNullableByte);
        response.Data.Attributes.TestSignedByte.Should().Be(newContainer.TestSignedByte);
        response.Data.Attributes.TestNullableSignedByte.Should().Be(newContainer.TestNullableSignedByte);
        response.Data.Attributes.TestInt16.Should().Be(newContainer.TestInt16);
        response.Data.Attributes.TestNullableInt16.Should().Be(newContainer.TestNullableInt16);
        response.Data.Attributes.TestUnsignedInt16.Should().Be(newContainer.TestUnsignedInt16);
        response.Data.Attributes.TestNullableUnsignedInt16.Should().Be(newContainer.TestNullableUnsignedInt16);
        response.Data.Attributes.TestInt32.Should().Be(newContainer.TestInt32);
        response.Data.Attributes.TestNullableInt32.Should().Be(newContainer.TestNullableInt32);
        response.Data.Attributes.TestUnsignedInt32.Should().Be(checked((int)newContainer.TestUnsignedInt32));
        response.Data.Attributes.TestNullableUnsignedInt32.Should().Be(checked((int?)newContainer.TestNullableUnsignedInt32));
        response.Data.Attributes.TestInt64.Should().Be(newContainer.TestInt64);
        response.Data.Attributes.TestNullableInt64.Should().Be(newContainer.TestNullableInt64);
        response.Data.Attributes.TestUnsignedInt64.Should().Be(checked((long)newContainer.TestUnsignedInt64));
        response.Data.Attributes.TestNullableUnsignedInt64.Should().Be(checked((long?)newContainer.TestNullableUnsignedInt64));
        response.Data.Attributes.TestInt128.Should().Be(newContainer.TestInt128.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestNullableInt128.Should().Be(newContainer.TestNullableInt128?.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestUnsignedInt128.Should().Be(newContainer.TestUnsignedInt128.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestNullableUnsignedInt128.Should().Be(newContainer.TestNullableUnsignedInt128?.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestBigInteger.Should().Be(newContainer.TestBigInteger.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestNullableBigInteger.Should().Be(newContainer.TestNullableBigInteger?.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestHalf.Should().Be(newContainer.TestHalf.AsFloat());
        response.Data.Attributes.TestNullableHalf.Should().Be(newContainer.TestNullableHalf?.AsFloat());
        response.Data.Attributes.TestFloat.Should().Be(newContainer.TestFloat);
        response.Data.Attributes.TestNullableFloat.Should().Be(newContainer.TestNullableFloat);
        response.Data.Attributes.TestDouble.Should().Be(newContainer.TestDouble);
        response.Data.Attributes.TestNullableDouble.Should().Be(newContainer.TestNullableDouble);
        response.Data.Attributes.TestDecimal.Should().Be((double)newContainer.TestDecimal);
        response.Data.Attributes.TestNullableDecimal.Should().Be((double?)newContainer.TestNullableDecimal);
        response.Data.Attributes.TestComplex.Should().Be(newContainer.TestComplex.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestNullableComplex.Should().Be(newContainer.TestNullableComplex?.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestChar.Should().Be(newContainer.TestChar.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestNullableChar.Should().Be(newContainer.TestNullableChar?.ToString(CultureInfo.InvariantCulture));
        response.Data.Attributes.TestString.Should().Be(newContainer.TestString);
        response.Data.Attributes.TestNullableString.Should().Be(newContainer.TestNullableString);
        response.Data.Attributes.TestRune.Should().Be(newContainer.TestRune.ToString());
        response.Data.Attributes.TestNullableRune.Should().Be(newContainer.TestNullableRune?.ToString());
        response.Data.Attributes.TestDateTimeOffset.Should().Be(newContainer.TestDateTimeOffset);
        response.Data.Attributes.TestNullableDateTimeOffset.Should().Be(newContainer.TestNullableDateTimeOffset);
        response.Data.Attributes.TestDateTime.Should().Be(newContainer.TestDateTime);
        response.Data.Attributes.TestNullableDateTime.Should().Be(newContainer.TestNullableDateTime);
        response.Data.Attributes.TestDateOnly.Should().Be((Date)newContainer.TestDateOnly);
        response.Data.Attributes.TestNullableDateOnly.Should().Be((Date?)newContainer.TestNullableDateOnly);
        response.Data.Attributes.TestTimeOnly.Should().Be((Time)newContainer.TestTimeOnly);
        response.Data.Attributes.TestNullableTimeOnly.Should().Be((Time?)newContainer.TestNullableTimeOnly);
        response.Data.Attributes.TestTimeSpan.Should().Be(newContainer.TestTimeSpan);
        response.Data.Attributes.TestNullableTimeSpan.Should().Be(newContainer.TestNullableTimeSpan);
        response.Data.Attributes.TestEnum.Should().Be(MapEnum<ServerDayOfWeek, ClientDayOfWeek>(newContainer.TestEnum));
        response.Data.Attributes.TestNullableEnum.Should().Be(MapEnum<ServerDayOfWeek, ClientDayOfWeek>(newContainer.TestNullableEnum));
        response.Data.Attributes.TestGuid.Should().Be(newContainer.TestGuid);
        response.Data.Attributes.TestNullableGuid.Should().Be(newContainer.TestNullableGuid);
        response.Data.Attributes.TestUri.Should().Be(newContainer.TestUri.ToString());
        response.Data.Attributes.TestNullableUri.Should().Be(newContainer.TestNullableUri?.ToString());
        response.Data.Attributes.TestIPAddress.Should().Be(newContainer.TestIPAddress.ToString());
        response.Data.Attributes.TestNullableIPAddress.Should().Be(newContainer.TestNullableIPAddress?.ToString());
        response.Data.Attributes.TestIPNetwork.Should().Be(newContainer.TestIPNetwork.ToString());
        response.Data.Attributes.TestNullableIPNetwork.Should().Be(newContainer.TestNullableIPNetwork?.ToString());
        response.Data.Attributes.TestVersion.Should().Be(newContainer.TestVersion.ToString());
        response.Data.Attributes.TestNullableVersion.Should().Be(newContainer.TestNullableVersion?.ToString());

        long newContainerId = long.Parse(response.Data.Id.Should().NotBeNull().And.Subject);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        var requestBody = new UpdateTypeContainerRequestDocument
        {
            Data = new DataInUpdateTypeContainerRequest
            {
                Type = TypeContainerResourceType.TypeContainers,
                Id = existingContainer.StringId!,
                Attributes = new AttributesInUpdateTypeContainerRequest
                {
                    TestNullableBoolean = null,
                    TestNullableByte = null,
                    TestNullableSignedByte = null,
                    TestNullableInt16 = null,
                    TestNullableUnsignedInt16 = null,
                    TestNullableInt32 = null,
                    TestNullableUnsignedInt32 = null,
                    TestNullableInt64 = null,
                    TestNullableUnsignedInt64 = null,
                    TestNullableInt128 = null,
                    TestNullableUnsignedInt128 = null,
                    TestNullableBigInteger = null,
                    TestNullableHalf = null,
                    TestNullableFloat = null,
                    TestNullableDouble = null,
                    TestNullableDecimal = null,
                    TestNullableComplex = null,
                    TestNullableChar = null,
                    TestNullableString = null,
                    TestNullableRune = null,
                    TestNullableDateTimeOffset = null,
                    TestNullableDateTime = null,
                    TestNullableDateOnly = null,
                    TestNullableTimeOnly = null,
                    TestNullableTimeSpan = null,
                    TestNullableEnum = null,
                    TestNullableGuid = null,
                    TestNullableUri = null,
                    TestNullableIPAddress = null,
                    TestNullableIPNetwork = null,
                    TestNullableVersion = null
                }
            }
        };

        // Act
        PrimaryTypeContainerResponseDocument? response = await apiClient.TypeContainers[existingContainer.StringId!].PatchAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.TestNullableBoolean.Should().BeNull();
        response.Data.Attributes.TestNullableByte.Should().BeNull();
        response.Data.Attributes.TestNullableSignedByte.Should().BeNull();
        response.Data.Attributes.TestNullableInt16.Should().BeNull();
        response.Data.Attributes.TestNullableUnsignedInt16.Should().BeNull();
        response.Data.Attributes.TestNullableInt32.Should().BeNull();
        response.Data.Attributes.TestNullableUnsignedInt32.Should().BeNull();
        response.Data.Attributes.TestNullableInt64.Should().BeNull();
        response.Data.Attributes.TestNullableUnsignedInt64.Should().BeNull();
        response.Data.Attributes.TestNullableInt128.Should().BeNull();
        response.Data.Attributes.TestNullableUnsignedInt128.Should().BeNull();
        response.Data.Attributes.TestNullableBigInteger.Should().BeNull();
        response.Data.Attributes.TestNullableHalf.Should().BeNull();
        response.Data.Attributes.TestNullableFloat.Should().BeNull();
        response.Data.Attributes.TestNullableDouble.Should().BeNull();
        response.Data.Attributes.TestNullableDecimal.Should().BeNull();
        response.Data.Attributes.TestNullableComplex.Should().BeNull();
        response.Data.Attributes.TestNullableChar.Should().BeNull();
        response.Data.Attributes.TestNullableString.Should().BeNull();
        response.Data.Attributes.TestNullableRune.Should().BeNull();
        response.Data.Attributes.TestNullableDateTimeOffset.Should().BeNull();
        response.Data.Attributes.TestNullableDateTime.Should().BeNull();
        response.Data.Attributes.TestNullableDateOnly.Should().BeNull();
        response.Data.Attributes.TestNullableTimeOnly.Should().BeNull();
        response.Data.Attributes.TestNullableTimeSpan.Should().BeNull();
        response.Data.Attributes.TestNullableEnum.Should().BeNull();
        response.Data.Attributes.TestNullableGuid.Should().BeNull();
        response.Data.Attributes.TestNullableUri.Should().BeNull();
        response.Data.Attributes.TestNullableIPAddress.Should().BeNull();
        response.Data.Attributes.TestNullableIPNetwork.Should().BeNull();
        response.Data.Attributes.TestNullableVersion.Should().BeNull();

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        var requestBody = new UpdateTypeContainerRequestDocument
        {
            Data = new DataInUpdateTypeContainerRequest
            {
                Type = TypeContainerResourceType.TypeContainers,
                Id = existingContainer.StringId!,
                Attributes = new AttributesInUpdateTypeContainerRequest()
            }
        };

        SetAttributeValueInUpdateRequestToNull(requestBody.Data.Attributes, propertyName);

        // Act
        Func<Task> action = async () => await apiClient.TypeContainers[existingContainer.StringId!].PatchAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("422");

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
    [InlineData(nameof(TypeContainer.TestInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestUnsignedInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestNullableUnsignedInt128), "The input string 'invalid' was not in a correct format.")]
    [InlineData(nameof(TypeContainer.TestBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestNullableBigInteger), "The value could not be parsed.")]
    [InlineData(nameof(TypeContainer.TestComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestNullableComplex), "Arithmetic operation resulted in an overflow.")]
    [InlineData(nameof(TypeContainer.TestIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPAddress), "An invalid IP address was specified.")]
    [InlineData(nameof(TypeContainer.TestIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestNullableIPNetwork), "An invalid IP network was specified.")]
    [InlineData(nameof(TypeContainer.TestVersion), "The JSON value is not in a supported Version format.")]
    [InlineData(nameof(TypeContainer.TestNullableVersion), "The JSON value is not in a supported Version format.")]
    public async Task Cannot_update_resource_with_attribute_set_to_invalid_value(string propertyName, string innerParseError)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        var requestBody = new UpdateTypeContainerRequestDocument
        {
            Data = new DataInUpdateTypeContainerRequest
            {
                Type = TypeContainerResourceType.TypeContainers,
                Id = existingContainer.StringId!,
                Attributes = new AttributesInUpdateTypeContainerRequest()
            }
        };

        SetAttributeValueInUpdateRequestToInvalid(requestBody.Data.Attributes, propertyName);

        // Act
        Func<Task> action = async () => await apiClient.TypeContainers[existingContainer.StringId!].PatchAsync(requestBody);

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("422");
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Detail.Should().Be(GetExpectedConverterErrorMessage(propertyName, "invalid"));
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be($"/data/attributes/{propertyName.Camelize()}");
        error.Meta.Should().NotBeNull();

        error.Meta.AdditionalData.Should().ContainKey("stackTrace").WhoseValue.Should().BeOfType<UntypedArray>().Subject.With(array =>
        {
            string stackTrace = string.Join(Environment.NewLine, array.GetValue().Select(item => item.Should().BeOfType<UntypedString>().Subject.GetValue()));
            stackTrace.Should().Contain(innerParseError);
        });
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
    public async Task Can_filter_with_valid_value(string propertyName)
    {
        // Arrange
        TypeContainer existingContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TypeContainer>();
            dbContext.TypeContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        string filterValue = GetFilterValue(existingContainer, propertyName);

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["filter"] = $"equals({propertyName.Camelize()},'{filterValue}')"
        });

        // Act
        TypeContainerCollectionResponseDocument? response = await apiClient.TypeContainers.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(existingContainer.StringId);
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
    public async Task Can_filter_with_null_value(string propertyName)
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["filter"] = $"equals({propertyName.Camelize()},null)"
        });

        // Act
        TypeContainerCollectionResponseDocument? response = await apiClient.TypeContainers.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
        response.Data.ElementAt(0).Id.Should().Be(existingContainer.StringId);
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
    public async Task Cannot_filter_with_null_value(string propertyName)
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["filter"] = $"equals({propertyName.Camelize()},null)"
        });

        // Act
        Func<Task> action = async () => await apiClient.TypeContainers.GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("400");
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
    public async Task Cannot_filter_with_invalid_value(string propertyName, string innerParseError)
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        AttributeTypesClient apiClient = new(requestAdapter);

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["filter"] = $"equals({propertyName.Camelize()},'invalid')"
        });

        // Act
        Func<Task> action = async () => await apiClient.TypeContainers.GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("400");
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().StartWith($"{GetExpectedQueryStringErrorMessage(propertyName, "invalid")} Failed at position");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
        error.Meta.Should().NotBeNull();

        error.Meta.AdditionalData.Should().ContainKey("stackTrace").WhoseValue.Should().BeOfType<UntypedArray>().Subject.With(array =>
        {
            string stackTrace = string.Join(Environment.NewLine, array.GetValue().Select(item => item.Should().BeOfType<UntypedString>().Subject.GetValue()));
            stackTrace.Should().Contain(innerParseError);
        });
    }

    private static void SetAttributeValueInUpdateRequestToNull(AttributesInUpdateTypeContainerRequest attributes, string propertyName)
    {
        MethodInfo? propertySetter = typeof(AttributesInUpdateTypeContainerRequest).GetProperty(propertyName)?.SetMethod;

        if (propertySetter == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        propertySetter.Invoke(attributes, [null]);
    }

    private static void SetAttributeValueInUpdateRequestToInvalid(AttributesInUpdateTypeContainerRequest attributes, string propertyName)
    {
        MethodInfo? propertySetter = typeof(AttributesInUpdateTypeContainerRequest).GetProperty(propertyName)?.SetMethod;

        if (propertySetter == null)
        {
            throw new InvalidOperationException($"Unknown property '{propertyName}'.");
        }

        propertySetter.Invoke(attributes, ["invalid"]);
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
        return converter != null ? converter(typedValue) : (string)RuntimeTypeConverter.ConvertType(typedValue, typeof(string))!;
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

    [return: NotNullIfNotNull(nameof(fromEnum))]
    private static TToEnum? MapEnum<TFromEnum, TToEnum>(TFromEnum? fromEnum)
        where TFromEnum : struct, Enum
        where TToEnum : struct, Enum
    {
        if (fromEnum == null)
        {
            return default(TToEnum);
        }

        string stringValue = fromEnum.Value.ToString("G");
        return Enum.Parse<TToEnum>(stringValue, false);
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
