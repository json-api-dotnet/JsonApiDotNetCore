using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.IdObfuscation;

public sealed class IdObfuscationTests : IClassFixture<OpenApiTestContext<ObfuscationStartup, ObfuscationDbContext>>
{
    private readonly OpenApiTestContext<ObfuscationStartup, ObfuscationDbContext> _testContext;

    public IdObfuscationTests(OpenApiTestContext<ObfuscationStartup, ObfuscationDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<BankAccountsController>();
        testContext.UseController<DebitCardsController>();
        testContext.UseController<OperationsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData("/bankAccounts/{id}.get")]
    [InlineData("/bankAccounts/{id}.head")]
    [InlineData("/bankAccounts/{id}.patch")]
    [InlineData("/bankAccounts/{id}.delete")]
    [InlineData("/bankAccounts/{id}/cards.get")]
    [InlineData("/bankAccounts/{id}/cards.head")]
    [InlineData("/bankAccounts/{id}/relationships/cards.get")]
    [InlineData("/bankAccounts/{id}/relationships/cards.head")]
    [InlineData("/bankAccounts/{id}/relationships/cards.post")]
    [InlineData("/bankAccounts/{id}/relationships/cards.patch")]
    [InlineData("/bankAccounts/{id}/relationships/cards.delete")]
    [InlineData("/debitCards/{id}.get")]
    [InlineData("/debitCards/{id}.head")]
    [InlineData("/debitCards/{id}.patch")]
    [InlineData("/debitCards/{id}.delete")]
    [InlineData("/debitCards/{id}/account.get")]
    [InlineData("/debitCards/{id}/account.head")]
    [InlineData("/debitCards/{id}/relationships/account.get")]
    [InlineData("/debitCards/{id}/relationships/account.head")]
    [InlineData("/debitCards/{id}/relationships/account.patch")]
    public async Task Hides_underlying_ID_type_in_path_parameter(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"paths.{endpointPath}.parameters").With(parametersElement =>
        {
            parametersElement.EnumerateArray().Should().ContainSingle(parameterElement => parameterElement.GetProperty("name").ValueEquals("id")).Subject
                .With(parameterElement =>
                {
                    JsonElement schemaElement = parameterElement.Should().ContainPath("schema");

                    schemaElement.ToString().Should().BeJson("""
                        {
                          "type": "string"
                        }
                        """);
                });
        });
    }

    [Theory]
    [InlineData("bankAccountCardsRelationshipIdentifier", false)]
    [InlineData("bankAccountIdentifierInRequest", true)]
    [InlineData("bankAccountIdentifierInResponse", false)]
    [InlineData("dataInBankAccountResponse", true)]
    [InlineData("dataInDebitCardResponse", true)]
    [InlineData("dataInUpdateBankAccountRequest", true)]
    [InlineData("dataInUpdateDebitCardRequest", true)]
    [InlineData("debitCardAccountRelationshipIdentifier", false)]
    [InlineData("debitCardIdentifierInRequest", true)]
    [InlineData("debitCardIdentifierInResponse", false)]
    public async Task Hides_underlying_ID_type_in_component_schema(string schemaId, bool isWrapped)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string path = isWrapped ? $"components.schemas.{schemaId}.allOf[1].properties.id" : $"components.schemas.{schemaId}.properties.id";

        document.Should().ContainPath(path).With(propertiesElement =>
        {
            propertiesElement.ToString().Should().BeJson("""
                {
                  "type": "string"
                }
                """);
        });
    }
}
