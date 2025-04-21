using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Documentation;

public sealed class ErrorResponseTests : IClassFixture<OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext>>
{
    private const string EscapedJsonApiMediaType = "['application/vnd.api+json; ext=openapi']";
    private const string EscapedOperationsMediaType = "['application/vnd.api+json; ext=atomic; ext=openapi']";

    private readonly OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext> _testContext;

    public ErrorResponseTests(OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<SkyscrapersController>();
        testContext.UseController<ElevatorsController>();
        testContext.UseController<SpacesController>();
        testContext.UseController<OperationsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Applicable_error_status_codes_with_schema_are_provided_on_endpoints()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./skyscrapers").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(1);

                errorStatusCodeProperties[0].Name.Should().Be("400");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            skyscrapersElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(1);

                errorStatusCodeProperties[0].Name.Should().Be("400");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });

            skyscrapersElement.Should().ContainPath("post.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}").With(idElement =>
        {
            idElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            idElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });

            idElement.Should().ContainPath("patch.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            idElement.Should().ContainPath("delete.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(1);

                errorStatusCodeProperties[0].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/elevator").With(elevatorElement =>
        {
            elevatorElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            elevatorElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/elevator").With(elevatorElement =>
        {
            elevatorElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            elevatorElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });

            elevatorElement.Should().ContainPath("patch.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/spaces").With(spacesElement =>
        {
            spacesElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            spacesElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/spaces").With(spacesElement =>
        {
            spacesElement.Should().ContainPath("get.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            spacesElement.Should().ContainPath("head.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(2);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");

                errorStatusCodeProperties.Should().AllSatisfy(property => property.Value.Should().NotContainPath("content"));
            });

            spacesElement.Should().ContainPath("post.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            spacesElement.Should().ContainPath("patch.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });

            spacesElement.Should().ContainPath("delete.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(4);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("404");
                errorStatusCodeProperties[2].Name.Should().Be("409");
                errorStatusCodeProperties[3].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
            });
        });

        document.Should().ContainPath("paths./operations").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("post.responses").With(responsesElement =>
            {
                JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
                errorStatusCodeProperties.Should().HaveCount(5);

                errorStatusCodeProperties[0].Name.Should().Be("400");
                errorStatusCodeProperties[1].Name.Should().Be("403");
                errorStatusCodeProperties[2].Name.Should().Be("404");
                errorStatusCodeProperties[3].Name.Should().Be("409");
                errorStatusCodeProperties[4].Name.Should().Be("422");

                errorStatusCodeProperties.Should().AllSatisfy(property =>
                    property.Value.Should().ContainPath($"content.{EscapedOperationsMediaType}.schema.$ref")
                        .ShouldBeSchemaReferenceId("errorResponseDocument"));
            });
        });
    }

    [Fact]
    public async Task Forbidden_status_is_added_when_client_generated_IDs_are_disabled()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./elevators.post.responses").With(responsesElement =>
        {
            JsonProperty[] errorStatusCodeProperties = responsesElement.EnumerateObject().Where(IsErrorStatusCode).ToArray();
            errorStatusCodeProperties.Should().HaveCount(5);

            errorStatusCodeProperties[0].Name.Should().Be("400");
            errorStatusCodeProperties[1].Name.Should().Be("403");
            errorStatusCodeProperties[2].Name.Should().Be("404");
            errorStatusCodeProperties[3].Name.Should().Be("409");
            errorStatusCodeProperties[4].Name.Should().Be("422");

            errorStatusCodeProperties.Should().AllSatisfy(property =>
                property.Value.Should().ContainPath($"content.{EscapedJsonApiMediaType}.schema.$ref").ShouldBeSchemaReferenceId("errorResponseDocument"));
        });
    }

    private static bool IsErrorStatusCode(JsonProperty statusCodeProperty)
    {
        return int.TryParse(statusCodeProperty.Name, out int statusCodeValue) && statusCodeValue >= 400;
    }
}
