using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

// @formatter:max_line_length 250

namespace OpenApiTests.Documentation;

public sealed class DocumentationTests : IClassFixture<OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext>>
{
    private const string ResourceTextQueryString =
        "For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.";

    private const string RelationshipTextQueryString =
        "For syntax, see the documentation for the [`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.";

    private readonly OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext> _testContext;

    public DocumentationTests(OpenApiTestContext<DocumentationStartup<DocumentationDbContext>, DocumentationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SkyscrapersController>();
        testContext.UseController<ElevatorsController>();
        testContext.UseController<SpacesController>();
        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task API_is_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("info").With(infoElement =>
        {
            infoElement.Should().HaveProperty("title", "Skyscrapers of the world");
            infoElement.Should().HaveProperty("description", "A JSON:API service for managing skyscrapers.");
            infoElement.Should().HaveProperty("version", "v1");

            infoElement.Should().ContainPath("contact").With(contactElement =>
            {
                contactElement.Should().HaveProperty("name", "Report issues");
                contactElement.Should().HaveProperty("url", "https://github.com/json-api-dotnet/JsonApiDotNetCore/issues");
            });

            infoElement.Should().ContainPath("license").With(contactElement =>
            {
                contactElement.Should().HaveProperty("name", "MIT License");
                contactElement.Should().HaveProperty("url", "https://licenses.nuget.org/MIT");
            });
        });
    }

    [Fact]
    public async Task Endpoints_are_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./skyscrapers").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves a collection of skyscrapers.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[1].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[1].in", "header");
                    parametersElement.Should().HaveProperty("[1].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(3);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found skyscrapers, or an empty array if none were found.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves a collection of skyscrapers without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[1].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[1].in", "header");
                    parametersElement.Should().HaveProperty("[1].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(3);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                });
            });

            skyscrapersElement.Should().ContainPath("post").With(postElement =>
            {
                postElement.Should().HaveProperty("summary", "Creates a new skyscraper.");

                postElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", ResourceTextQueryString);
                });

                postElement.Should().HaveProperty("requestBody.description", "The attributes and relationships of the skyscraper to create.");

                postElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(6);
                    responsesElement.Should().HaveProperty("201.description", "The skyscraper was successfully created, which resulted in additional changes. The newly created skyscraper is returned.");
                    responsesElement.Should().HaveProperty("201.headers.Location.description", "The URL at which the newly created skyscraper can be retrieved.");
                    responsesElement.Should().HaveProperty("204.description", "The skyscraper was successfully created, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid or the request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "A related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                    responsesElement.Should().HaveProperty("422.description", "Validation of the request body failed.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}").With(idElement =>
        {
            idElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves an individual skyscraper by its identifier.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found skyscraper.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            idElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves an individual skyscraper by its identifier without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            idElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Updates an existing skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to update.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                });

                patchElement.Should().HaveProperty("requestBody.description", "The attributes and relationships of the skyscraper to update. Omitted fields are left unchanged.");

                patchElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(6);
                    responsesElement.Should().HaveProperty("200.description", "The skyscraper was successfully updated, which resulted in additional changes. The updated skyscraper is returned.");
                    responsesElement.Should().HaveProperty("204.description", "The skyscraper was successfully updated, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid or the request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "A resource type or identifier in the request body is incompatible.");
                    responsesElement.Should().HaveProperty("422.description", "Validation of the request body failed.");
                });
            });

            idElement.Should().ContainPath("delete").With(deleteElement =>
            {
                deleteElement.Should().HaveProperty("summary", "Deletes an existing skyscraper by its identifier.");

                deleteElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to delete.");
                });

                deleteElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(2);
                    responsesElement.Should().HaveProperty("204.description", "The skyscraper was successfully deleted.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/elevator").With(elevatorElement =>
        {
            elevatorElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related elevator of an individual skyscraper's elevator relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found elevator, or `null` if it was not found.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            elevatorElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related elevator of an individual skyscraper's elevator relationship without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/elevator").With(elevatorElement =>
        {
            elevatorElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related elevator identity of an individual skyscraper's elevator relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator identity to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", RelationshipTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found elevator identity, or `null` if it was not found.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            elevatorElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related elevator identity of an individual skyscraper's elevator relationship without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator identity to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", RelationshipTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            elevatorElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Clears or assigns an existing elevator to the elevator relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose elevator relationship to assign or clear.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identity of the elevator to assign to the elevator relationship, or `null` to clear the relationship.");

                patchElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("204.description", "The elevator relationship was successfully updated, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/spaces").With(spacesElement =>
        {
            spacesElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related spaces of an individual skyscraper's spaces relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related spaces to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found spaces, or an empty array if none were found.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            spacesElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related spaces of an individual skyscraper's spaces relationship without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related spaces to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", ResourceTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/spaces").With(spacesElement =>
        {
            spacesElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related space identities of an individual skyscraper's spaces relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related space identities to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", RelationshipTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                getElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "Successfully returns the found space identities, or an empty array if none were found.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            spacesElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related space identities of an individual skyscraper's spaces relationship without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(3);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related space identities to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", RelationshipTextQueryString);
                    parametersElement.Should().HaveProperty("[2].name", "If-None-Match");
                    parametersElement.Should().HaveProperty("[2].in", "header");
                    parametersElement.Should().HaveProperty("[2].description", "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");
                });

                headElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responsesElement.Should().HaveProperty("200.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("200.headers.Content-Length.description", "Size of the HTTP response body, in bytes.");
                    responsesElement.Should().HaveProperty("304.description", "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.");
                    responsesElement.Should().HaveProperty("304.headers.ETag.description", "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");
                    responsesElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            spacesElement.Should().ContainPath("post").With(postElement =>
            {
                postElement.Should().HaveProperty("summary", "Adds existing spaces to the spaces relationship of an individual skyscraper.");

                postElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to add spaces to.");
                });

                postElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to add to the spaces relationship.");

                postElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("204.description", "The spaces were successfully added, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                });
            });

            spacesElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Assigns existing spaces to the spaces relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose spaces relationship to assign.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to assign to the spaces relationship, or an empty array to clear the relationship.");

                patchElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("204.description", "The spaces relationship was successfully updated, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                });
            });

            spacesElement.Should().ContainPath("delete").With(deleteElement =>
            {
                deleteElement.Should().HaveProperty("summary", "Removes existing spaces from the spaces relationship of an individual skyscraper.");

                deleteElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().Should().HaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to remove spaces from.");
                });

                deleteElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to remove from the spaces relationship.");

                deleteElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(4);
                    responsesElement.Should().HaveProperty("204.description", "The spaces were successfully removed, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                });
            });
        });

        document.Should().ContainPath("paths./operations").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("post").With(postElement =>
            {
                postElement.Should().HaveProperty("summary", "Performs multiple mutations in a linear and atomic manner.");

                postElement.Should().HaveProperty("requestBody.description", "An array of mutation operations. For syntax, see the [Atomic Operations documentation](https://jsonapi.org/ext/atomic/).");

                postElement.Should().ContainPath("responses").With(responsesElement =>
                {
                    responsesElement.EnumerateObject().Should().HaveCount(7);
                    responsesElement.Should().HaveProperty("200.description", "All operations were successfully applied, which resulted in additional changes.");
                    responsesElement.Should().HaveProperty("204.description", "All operations were successfully applied, which did not result in additional changes.");
                    responsesElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responsesElement.Should().HaveProperty("403.description", "An operation is not accessible or a client-generated ID is used.");
                    responsesElement.Should().HaveProperty("404.description", "A resource or a related resource does not exist.");
                    responsesElement.Should().HaveProperty("409.description", "The request body contains conflicting information or another resource with the same ID already exists.");
                    responsesElement.Should().HaveProperty("422.description", "Validation of the request body failed.");
                });
            });
        });
    }

    [Fact]
    public async Task Resource_types_are_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().HaveProperty("dataInUpdateElevatorRequest.description", "An elevator within a skyscraper.");
            schemasElement.Should().HaveProperty("dataInCreateElevatorRequest.description", "An elevator within a skyscraper.");
            schemasElement.Should().HaveProperty("elevatorDataInResponse.description", "An elevator within a skyscraper.");

            schemasElement.Should().HaveProperty("dataInUpdateSkyscraperRequest.description", "A tall, continuously habitable building having multiple floors.");
            schemasElement.Should().HaveProperty("dataInCreateSkyscraperRequest.description", "A tall, continuously habitable building having multiple floors.");
            schemasElement.Should().HaveProperty("skyscraperDataInResponse.description", "A tall, continuously habitable building having multiple floors.");

            schemasElement.Should().HaveProperty("dataInUpdateSpaceRequest.description", "A space within a skyscraper, such as an office, hotel, residential space, or retail space.");
            schemasElement.Should().HaveProperty("dataInCreateSpaceRequest.description", "A space within a skyscraper, such as an office, hotel, residential space, or retail space.");
            schemasElement.Should().HaveProperty("spaceDataInResponse.description", "A space within a skyscraper, such as an office, hotel, residential space, or retail space.");
        });
    }

    [Fact]
    public async Task Attributes_are_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().HaveProperty("attributesInUpdateElevatorRequest.properties.floorCount.description", "The number of floors this elevator provides access to.");
            schemasElement.Should().HaveProperty("attributesInCreateElevatorRequest.properties.floorCount.description", "The number of floors this elevator provides access to.");
            schemasElement.Should().HaveProperty("elevatorAttributesInResponse.properties.floorCount.description", "The number of floors this elevator provides access to.");

            schemasElement.Should().HaveProperty("attributesInUpdateSkyscraperRequest.properties.heightInMeters.description", "The height of this building, in meters.");
            schemasElement.Should().HaveProperty("attributesInCreateSkyscraperRequest.properties.heightInMeters.description", "The height of this building, in meters.");
            schemasElement.Should().HaveProperty("skyscraperAttributesInResponse.properties.heightInMeters.description", "The height of this building, in meters.");

            schemasElement.Should().ContainPath("attributesInUpdateSpaceRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("floorNumber.description", "The floor number on which this space resides.");
                propertiesElement.Should().HaveProperty("kind.description", "The kind of this space.");
            });

            schemasElement.Should().ContainPath("attributesInCreateSpaceRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("floorNumber.description", "The floor number on which this space resides.");
                propertiesElement.Should().HaveProperty("kind.description", "The kind of this space.");
            });

            schemasElement.Should().ContainPath("spaceAttributesInResponse.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("floorNumber.description", "The floor number on which this space resides.");
                propertiesElement.Should().HaveProperty("kind.description", "The kind of this space.");
            });
        });
    }

    [Fact]
    public async Task Relationships_are_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().HaveProperty("relationshipsInUpdateElevatorRequest.properties.existsIn.description", "The skyscraper this elevator exists in.");
            schemasElement.Should().HaveProperty("relationshipsInCreateElevatorRequest.properties.existsIn.description", "The skyscraper this elevator exists in.");
            schemasElement.Should().HaveProperty("elevatorRelationshipsInResponse.properties.existsIn.description", "The skyscraper this elevator exists in.");

            schemasElement.Should().ContainPath("relationshipsInUpdateSkyscraperRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().ContainPath("relationshipsInCreateSkyscraperRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().ContainPath("skyscraperRelationshipsInResponse.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().HaveProperty("relationshipsInUpdateSpaceRequest.properties.existsIn.description", "The skyscraper this space exists in.");
            schemasElement.Should().HaveProperty("relationshipsInCreateSpaceRequest.properties.existsIn.description", "The skyscraper this space exists in.");
            schemasElement.Should().HaveProperty("spaceRelationshipsInResponse.properties.existsIn.description", "The skyscraper this space exists in.");
        });
    }

    [Fact]
    public async Task Enums_are_documented()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().HaveProperty("spaceKind.description", "Lists the various kinds of spaces within a skyscraper.");
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
            responsesElement.Should().HaveProperty("403.description", "Client-generated IDs cannot be used at this endpoint.");
        });
    }
}
