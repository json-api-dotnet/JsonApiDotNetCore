using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.AttributeTypes;

public sealed class AttributeTypeTests : IClassFixture<OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    public static readonly TheoryData<string> SchemaNames =
#pragma warning disable CA1825 // Avoid zero-length array allocations
        // Justification: Workaround for bug https://github.com/dotnet/roslyn/issues/82484.
        [
            "attributesInCreateTypeContainerRequest",
            "attributesInUpdateTypeContainerRequest",
            "attributesInTypeContainerResponse"
        ];
#pragma warning restore CA1825 // Avoid zero-length array allocations

    private readonly OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;

    public AttributeTypeTests(OpenApiTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<TypeContainersController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [MemberData(nameof(SchemaNames))]
    public async Task Types_produce_expected_schemas(string modelName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"components.schemas.{modelName}.allOf[1].properties").Should().BeJson("""
            {
              "testBoolean": {
                "type": "boolean"
              },
              "testNullableBoolean": {
                "type": "boolean",
                "nullable": true
              },
              "testByte": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableByte": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testSignedByte": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableSignedByte": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testInt16": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableInt16": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testUnsignedInt16": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableUnsignedInt16": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testInt32": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableInt32": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testUnsignedInt32": {
                "type": "integer",
                "format": "int32"
              },
              "testNullableUnsignedInt32": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "testInt64": {
                "type": "integer",
                "format": "int64"
              },
              "testNullableInt64": {
                "type": "integer",
                "format": "int64",
                "nullable": true
              },
              "testUnsignedInt64": {
                "type": "integer",
                "format": "int64"
              },
              "testNullableUnsignedInt64": {
                "type": "integer",
                "format": "int64",
                "nullable": true
              },
              "testInt128": {
                "type": "string"
              },
              "testNullableInt128": {
                "type": "string",
                "nullable": true
              },
              "testUnsignedInt128": {
                "type": "string"
              },
              "testNullableUnsignedInt128": {
                "type": "string",
                "nullable": true
              },
              "testBigInteger": {
                "type": "string"
              },
              "testNullableBigInteger": {
                "type": "string",
                "nullable": true
              },
              "testHalf": {
                "type": "number",
                "format": "float"
              },
              "testNullableHalf": {
                "type": "number",
                "format": "float",
                "nullable": true
              },
              "testFloat": {
                "type": "number",
                "format": "float"
              },
              "testNullableFloat": {
                "type": "number",
                "format": "float",
                "nullable": true
              },
              "testDouble": {
                "type": "number",
                "format": "double"
              },
              "testNullableDouble": {
                "type": "number",
                "format": "double",
                "nullable": true
              },
              "testDecimal": {
                "type": "number",
                "format": "double"
              },
              "testNullableDecimal": {
                "type": "number",
                "format": "double",
                "nullable": true
              },
              "testComplex": {
                "type": "string"
              },
              "testNullableComplex": {
                "type": "string",
                "nullable": true
              },
              "testChar": {
                "type": "string"
              },
              "testNullableChar": {
                "type": "string",
                "nullable": true
              },
              "testString": {
                "type": "string"
              },
              "testNullableString": {
                "type": "string",
                "nullable": true
              },
              "testRune": {
                "maxLength": 4,
                "type": "string"
              },
              "testNullableRune": {
                "maxLength": 4,
                "type": "string",
                "nullable": true
              },
              "testDateTimeOffset": {
                "type": "string",
                "format": "date-time"
              },
              "testNullableDateTimeOffset": {
                "type": "string",
                "format": "date-time",
                "nullable": true
              },
              "testDateTime": {
                "type": "string",
                "format": "date-time"
              },
              "testNullableDateTime": {
                "type": "string",
                "format": "date-time",
                "nullable": true
              },
              "testDateOnly": {
                "type": "string",
                "format": "date"
              },
              "testNullableDateOnly": {
                "type": "string",
                "format": "date",
                "nullable": true
              },
              "testTimeOnly": {
                "type": "string",
                "format": "time"
              },
              "testNullableTimeOnly": {
                "type": "string",
                "format": "time",
                "nullable": true
              },
              "testTimeSpan": {
                "type": "string",
                "format": "duration"
              },
              "testNullableTimeSpan": {
                "type": "string",
                "format": "duration",
                "nullable": true
              },
              "testEnum": {
                "allOf": [
                  {
                    "$ref": "#/components/schemas/dayOfWeek"
                  }
                ]
              },
              "testNullableEnum": {
                "allOf": [
                  {
                    "$ref": "#/components/schemas/dayOfWeek"
                  }
                ],
                "nullable": true
              },
              "testGuid": {
                "type": "string",
                "format": "uuid"
              },
              "testNullableGuid": {
                "type": "string",
                "format": "uuid",
                "nullable": true
              },
              "testUri": {
                "type": "string",
                "format": "uri"
              },
              "testNullableUri": {
                "type": "string",
                "format": "uri",
                "nullable": true
              },
              "testIPAddress": {
                "type": "string",
                "format": "ipv4"
              },
              "testNullableIPAddress": {
                "type": "string",
                "format": "ipv4",
                "nullable": true
              },
              "testIPNetwork": {
                "type": "string"
              },
              "testNullableIPNetwork": {
                "type": "string",
                "nullable": true
              },
              "testVersion": {
                "type": "string"
              },
              "testNullableVersion": {
                "type": "string",
                "nullable": true
              }
            }
            """);

        document.Should().ContainPath("components.schemas.dayOfWeek").Should().BeJson("""
            {
              "enum": [
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday"
              ],
              "type": "string"
            }
            """);
    }
}
