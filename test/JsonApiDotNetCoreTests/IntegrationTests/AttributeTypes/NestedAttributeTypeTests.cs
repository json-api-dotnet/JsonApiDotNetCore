using System.Net;
using System.Text.Json;
using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AttributeTypes;

public sealed class NestedAttributeTypeTests : IClassFixture<IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext>>
{
    // One entry per converter family: member name, JSON text of the value and whether the JSON value is a string.
    private static readonly Dictionary<string, (string Text, bool IsString)> ScalarCases = new()
    {
        [nameof(TypeContainerNested.TestInt128)] = ("12345678901234567890", true),
        [nameof(TypeContainerNested.TestUnsignedInt128)] = ("12345678901234567890", true),
        [nameof(TypeContainerNested.TestBigInteger)] = ("123456789012345678901234567890", true),
        [nameof(TypeContainerNested.TestHalf)] = ("1.5", false),
        [nameof(TypeContainerNested.TestRune)] = ("x", true),
        [nameof(TypeContainerNested.TestIPAddress)] = ("10.1.2.3", true),
        [nameof(TypeContainerNested.TestIPNetwork)] = ("10.1.0.0/16", true),
        [nameof(TypeContainerNested.TestVersion)] = ("1.2.3", true),
        [nameof(TypeContainerNested.TestUri)] = ("https://example.com/a", true),
        [nameof(TypeContainerNested.TestTimeSpan)] = ("01:30:00", true),
        [nameof(TypeContainerNested.TestDateOnly)] = ("2020-01-02", true),
        [nameof(TypeContainerNested.TestTimeOnly)] = ("01:02:03", true),
        [nameof(TypeContainerNested.TestDateTimeOffset)] = ("2020-01-02T03:04:05+00:00", true),
        [nameof(TypeContainerNested.TestGuid)] = ("5f3a5b6c-0000-4000-8000-000000000001", true),
        [nameof(TypeContainerNested.TestDecimal)] = ("12.5", false),
        [nameof(TypeContainerNested.TestEnum)] = ("Tuesday", true)
    };

    // One entry per supported collection: member name, two element values and whether the JSON elements are strings.
    private static readonly Dictionary<string, (string First, string Second, bool IsString)> CollectionCases = new()
    {
        [nameof(TypeContainerNested.TestInts)] = ("1", "2", false),
        [nameof(TypeContainerNested.TestBigIntegers)] = ("123456789012345678901234567890", "42", true),
        [nameof(TypeContainerNested.TestIPAddresses)] = ("10.1.2.3", "10.1.2.4", true),
        [nameof(TypeContainerNested.TestIPNetworks)] = ("10.1.0.0/16", "192.168.0.0/24", true),
        [nameof(TypeContainerNested.TestUris)] = ("https://example.com/a", "https://example.com/b", true),
        [nameof(TypeContainerNested.TestTimeSpans)] = ("01:30:00", "00:05:00", true),
        [nameof(TypeContainerNested.TestDateOnlys)] = ("2020-01-02", "2021-03-04", true),
        [nameof(TypeContainerNested.TestTimeOnlys)] = ("01:02:03", "04:05:06", true),
        [nameof(TypeContainerNested.TestDateTimeOffsets)] = ("2020-01-02T03:04:05+00:00", "2021-03-04T05:06:07+00:00", true),
        [nameof(TypeContainerNested.TestGuids)] = ("5f3a5b6c-0000-4000-8000-000000000001", "5f3a5b6c-0000-4000-8000-000000000002", true),
        [nameof(TypeContainerNested.TestDecimals)] = ("12.5", "0.25", false),
        [nameof(TypeContainerNested.TestEnums)] = ("Tuesday", "Friday", true)
    };

    private const string SkipPrefix = "EF Core 10 (model build): Unable to configure navigation '<ComplexType>.<Member>' of type 'List<";

    private const string SkipSuffix =
        ">' as complex types don't support navigations. Ignore this property using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.";

    private readonly IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> _testContext;
    private readonly AttributeTypesFakers _fakers = new();

    public static TheoryData<string> ScalarNames => [.. ScalarCases.Keys];
    public static TheoryData<string> CollectionNames => [.. CollectionCases.Keys];

    public static TheoryData<string> FilterableCollectionNames =>
        [.. CollectionCases.Keys.Except([nameof(TypeContainerNested.TestBigIntegers), nameof(TypeContainerNested.TestTimeOnlys)])];

    public NestedAttributeTypeTests(IntegrationTestContext<AttributeTypesStartup, AttributeTypesDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TypeContainersController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Theory]
    [MemberData(nameof(ScalarNames))]
    public async Task Can_round_trip_type_in_compound(string memberName)
    {
        // Arrange
        (string text, bool isString) = ScalarCases[memberName];
        TypeContainer existingContainer = await CreateContainerAsync();

        string requestBody = CreatePatchBody(existingContainer, memberName, ToJson(text, isString));
        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
        (HttpResponseMessage getResponse, string getResponseText) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(getResponseText);
        JsonElement member = GetNestedMember(document, memberName);
        member.ValueKind.Should().Be(isString ? JsonValueKind.String : JsonValueKind.Number);
        member.ToString().Should().Be(text);
    }

    [Theory]
    [MemberData(nameof(ScalarNames))]
    public async Task Can_filter_on_type_in_compound(string memberName)
    {
        // Arrange
        (string text, bool isString) = ScalarCases[memberName];
        TypeContainer existingContainer = await CreateContainerAsync();

        string requestBody = CreatePatchBody(existingContainer, memberName, ToJson(text, isString));

        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>($"/typeContainers/{existingContainer.StringId}", requestBody);
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string route = $"/typeContainers?filter=equals(nested.{memberName.Camelize()},'{Uri.EscapeDataString(text)}')";

        // Act
        (HttpResponseMessage httpResponse, string responseText) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(responseText);
        JsonElement data = document.RootElement.GetProperty("data");
        data.GetArrayLength().Should().Be(1);
        data[0].GetProperty("id").GetString().Should().Be(existingContainer.StringId);
    }

    [Theory]
    [MemberData(nameof(CollectionNames))]
    public async Task Can_round_trip_scalar_collection_of_type(string memberName)
    {
        // Arrange
        (string first, string second, bool isString) = CollectionCases[memberName];
        TypeContainer existingContainer = await CreateContainerAsync();

        string requestBody = CreatePatchBody(existingContainer, memberName, $"[{ToJson(first, isString)},{ToJson(second, isString)}]");
        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
        (HttpResponseMessage getResponse, string getResponseText) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(getResponseText);
        JsonElement member = GetNestedMember(document, memberName);
        member.EnumerateArray().Select(element => element.ToString()).Should().Equal(first, second);
    }

    [Theory]
    [MemberData(nameof(FilterableCollectionNames))]
    [InlineData(nameof(TypeContainerNested.TestBigIntegers), Skip = "KNOWN-ISSUE NEW-has-element-in-json: has(nested.testBigIntegers,equals(_,'42')) returns no rows.")]
    [InlineData(nameof(TypeContainerNested.TestTimeOnlys), Skip = "KNOWN-ISSUE NEW-has-element-in-json: has(nested.testTimeOnlys,equals(_,'04:05:06')) returns no rows.")]
    public async Task Can_filter_scalar_collection_elements_of_type(string memberName)
    {
        // Arrange
        (string first, string second, bool isString) = CollectionCases[memberName];
        TypeContainer existingContainer = await CreateContainerAsync();
        TypeContainer otherContainer = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TypeContainers.Add(otherContainer);
            await dbContext.SaveChangesAsync();
        });

        string requestBody = CreatePatchBody(existingContainer, memberName, $"[{ToJson(first, isString)},{ToJson(second, isString)}]");

        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>($"/typeContainers/{existingContainer.StringId}", requestBody);
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string route = $"/typeContainers?filter=has(nested.{memberName.Camelize()},equals(_,'{Uri.EscapeDataString(second)}'))";

        // Act
        (HttpResponseMessage httpResponse, string responseText) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(responseText);
        JsonElement data = document.RootElement.GetProperty("data");
        data.GetArrayLength().Should().Be(1);
        data[0].GetProperty("id").GetString().Should().Be(existingContainer.StringId);
    }

    [Theory]
    [InlineData(nameof(TypeContainerNested.TestInts), "1", "3", false)]
    [InlineData(nameof(TypeContainerNested.TestDateOnlys), "2020-01-02", "2021-03-04", true)]
    [InlineData(nameof(TypeContainerNested.TestTimeSpans), "01:30:00", "00:05:00", true)]
    public async Task Can_round_trip_null_elements_of_nullable_types(string memberName, string first, string last, bool isString)
    {
        // Arrange
        TypeContainer existingContainer = await CreateContainerAsync();

        string requestBody = CreatePatchBody(existingContainer, memberName, $"[{ToJson(first, isString)},null,{ToJson(last, isString)}]");
        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
        (HttpResponseMessage getResponse, string getResponseText) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(getResponseText);
        JsonElement[] elements = GetNestedMember(document, memberName).EnumerateArray().ToArray();

        elements.Should().HaveCount(3);
        elements[0].ToString().Should().Be(first);
        elements[1].ValueKind.Should().Be(JsonValueKind.Null);
        elements[2].ToString().Should().Be(last);
    }

    // Outcome list (I-AT-06): collections of these element types cannot be mapped inside a JSON column, because EF Core does not apply the value converter
    // to the elements of a primitive collection and instead treats the collection property as a navigation. The model fails to build at startup with the
    // message in the skip reasons, so the members are not part of TypeContainerNested. Scalar properties of the same types do work inside JSON.
    [Theory]
    [InlineData("TestInt128s", "1", "2", true, Skip = $"{SkipPrefix}Int128?{SkipSuffix}")]
    [InlineData("TestUnsignedInt128s", "1", "2", true, Skip = $"{SkipPrefix}UInt128?{SkipSuffix}")]
    [InlineData("TestHalfs", "1.5", "2.5", false, Skip = $"{SkipPrefix}Half?{SkipSuffix}")]
    [InlineData("TestRunes", "x", "y", true, Skip = $"{SkipPrefix}Rune?{SkipSuffix}")]
    [InlineData("TestVersions", "1.2.3", "4.5.6", true, Skip = $"{SkipPrefix}Version{SkipSuffix}")]
    [InlineData("TestComplexes", "<1; 2>", "<3; 4>", true, Skip = $"{SkipPrefix}Complex?{SkipSuffix}")]
    public async Task Can_round_trip_scalar_collection_of_unsupported_type(string memberName, string first, string second, bool isString)
    {
        // Arrange
        TypeContainer existingContainer = await CreateContainerAsync();

        string requestBody = CreatePatchBody(existingContainer, memberName, $"[{ToJson(first, isString)},{ToJson(second, isString)}]");
        string route = $"/typeContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage patchResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        patchResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    private async Task<TypeContainer> CreateContainerAsync()
    {
        TypeContainer container = _fakers.TypeContainer.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<TypeContainer>();
            dbContext.TypeContainers.Add(container);
            await dbContext.SaveChangesAsync();
        });

        return container;
    }

    private static string ToJson(string text, bool isString)
    {
        return isString ? $"\"{text}\"" : text;
    }

    private static string CreatePatchBody(TypeContainer container, string memberName, string memberJson)
    {
        return $$"""
            {
              "data": {
                "type": "typeContainers",
                "id": "{{container.StringId}}",
                "attributes": {
                  "nested": {
                    "{{memberName.Camelize()}}": {{memberJson}}
                  }
                }
              }
            }
            """;
    }

    private static JsonElement GetNestedMember(JsonDocument document, string memberName)
    {
        return document.RootElement.GetProperty("data").GetProperty("attributes").GetProperty("nested").GetProperty(memberName.Camelize());
    }
}
