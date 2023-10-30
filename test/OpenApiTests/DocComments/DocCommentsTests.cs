using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

// @formatter:max_line_length 250

namespace OpenApiTests.DocComments;

public sealed class DocCommentsTests : IClassFixture<OpenApiTestContext<DocCommentsStartup<DocCommentsDbContext>, DocCommentsDbContext>>
{
    private const string TextQueryString =
        "For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.";

    private readonly OpenApiTestContext<DocCommentsStartup<DocCommentsDbContext>, DocCommentsDbContext> _testContext;

    public DocCommentsTests(OpenApiTestContext<DocCommentsStartup<DocCommentsDbContext>, DocCommentsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SkyscrapersController>();
        testContext.UseController<ElevatorsController>();
        testContext.UseController<SpacesController>();
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
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(2);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found skyscrapers, or an empty array if none were found.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves a collection of skyscrapers without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(2);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                });
            });

            skyscrapersElement.Should().ContainPath("post").With(postElement =>
            {
                postElement.Should().HaveProperty("summary", "Creates a new skyscraper.");

                postElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "query");
                    parametersElement.Should().HaveProperty("[0].description", TextQueryString);
                });

                postElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(5);
                    responseElement.Should().HaveProperty("201.description", "The skyscraper was successfully created, which resulted in additional changes. The newly created skyscraper is returned.");
                    responseElement.Should().HaveProperty("204.description", "The skyscraper was successfully created, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid or the request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type in the request body is incompatible.");
                    responseElement.Should().HaveProperty("422.description", "Validation of the request body failed.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves an individual skyscraper by its identifier.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found skyscraper.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves an individual skyscraper by its identifier without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Updates an existing skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to update.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                patchElement.Should().HaveProperty("requestBody.description", "The attributes and relationships of the skyscraper to update. Omitted fields are left unchanged.");

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(6);
                    responseElement.Should().HaveProperty("200.description", "The skyscraper was successfully updated, which resulted in additional changes. The updated skyscraper is returned.");
                    responseElement.Should().HaveProperty("204.description", "The skyscraper was successfully updated, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper or a related resource does not exist.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid or the request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type or identifier in the request body is incompatible.");
                    responseElement.Should().HaveProperty("422.description", "Validation of the request body failed.");
                });
            });

            skyscrapersElement.Should().ContainPath("delete").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Deletes an existing skyscraper by its identifier.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to delete.");
                });

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(2);
                    responseElement.Should().HaveProperty("204.description", "The skyscraper was successfully deleted.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/elevator").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related elevator of an individual skyscraper's elevator relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found elevator, or `null` if it was not found.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related elevator of an individual skyscraper's elevator relationship without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/elevator").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related elevator identity of an individual skyscraper's elevator relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator identity to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found elevator identity, or `null` if it was not found.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related elevator identity of an individual skyscraper's elevator relationship without returning it.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related elevator identity to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Clears or assigns an existing elevator to the elevator relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose elevator relationship to assign or clear.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identity of the elevator to assign to the elevator relationship, or `null` to clear the relationship.");

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(4);
                    responseElement.Should().HaveProperty("204.description", "The elevator relationship was successfully updated, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                    responseElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type in the request body is incompatible.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/spaces").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related spaces of an individual skyscraper's spaces relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related spaces to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found spaces, or an empty array if none were found.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related spaces of an individual skyscraper's spaces relationship without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related spaces to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });
        });

        document.Should().ContainPath("paths./skyscrapers/{id}/relationships/spaces").With(skyscrapersElement =>
        {
            skyscrapersElement.Should().ContainPath("get").With(getElement =>
            {
                getElement.Should().HaveProperty("summary", "Retrieves the related space identities of an individual skyscraper's spaces relationship.");

                getElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related space identities to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                getElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "Successfully returns the found space identities, or an empty array if none were found.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("head").With(headElement =>
            {
                headElement.Should().HaveProperty("summary", "Retrieves the related space identities of an individual skyscraper's spaces relationship without returning them.");
                headElement.Should().HaveProperty("description", "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.");

                headElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(2);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose related space identities to retrieve.");
                    parametersElement.Should().HaveProperty("[1].in", "query");
                    parametersElement.Should().HaveProperty("[1].description", TextQueryString);
                });

                headElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(3);
                    responseElement.Should().HaveProperty("200.description", "The operation completed successfully.");
                    responseElement.Should().HaveProperty("400.description", "The query string is invalid.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                });
            });

            skyscrapersElement.Should().ContainPath("post").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Adds existing spaces to the spaces relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to add spaces to.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to add to the spaces relationship.");

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(4);
                    responseElement.Should().HaveProperty("204.description", "The spaces were successfully added, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                    responseElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type in the request body is incompatible.");
                });
            });

            skyscrapersElement.Should().ContainPath("patch").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Assigns existing spaces to the spaces relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper whose spaces relationship to assign.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to assign to the spaces relationship, or an empty array to clear the relationship.");

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(4);
                    responseElement.Should().HaveProperty("204.description", "The spaces relationship was successfully updated, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                    responseElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type in the request body is incompatible.");
                });
            });

            skyscrapersElement.Should().ContainPath("delete").With(patchElement =>
            {
                patchElement.Should().HaveProperty("summary", "Removes existing spaces from the spaces relationship of an individual skyscraper.");

                patchElement.Should().ContainPath("parameters").With(parametersElement =>
                {
                    parametersElement.EnumerateArray().ShouldHaveCount(1);
                    parametersElement.Should().HaveProperty("[0].in", "path");
                    parametersElement.Should().HaveProperty("[0].description", "The identifier of the skyscraper to remove spaces from.");
                });

                patchElement.Should().HaveProperty("requestBody.description", "The identities of the spaces to remove from the spaces relationship.");

                patchElement.Should().ContainPath("responses").With(responseElement =>
                {
                    responseElement.EnumerateObject().ShouldHaveCount(4);
                    responseElement.Should().HaveProperty("204.description", "The spaces were successfully removed, which did not result in additional changes.");
                    responseElement.Should().HaveProperty("404.description", "The skyscraper does not exist.");
                    responseElement.Should().HaveProperty("400.description", "The request body is missing or malformed.");
                    responseElement.Should().HaveProperty("409.description", "A resource type in the request body is incompatible.");
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
            schemasElement.Should().HaveProperty("elevatorDataInPatchRequest.description", "An elevator within a skyscraper.");
            schemasElement.Should().HaveProperty("elevatorDataInPostRequest.description", "An elevator within a skyscraper.");
            schemasElement.Should().HaveProperty("elevatorDataInResponse.description", "An elevator within a skyscraper.");

            schemasElement.Should().HaveProperty("skyscraperDataInPatchRequest.description", "A tall, continuously habitable building having multiple floors.");
            schemasElement.Should().HaveProperty("skyscraperDataInPostRequest.description", "A tall, continuously habitable building having multiple floors.");
            schemasElement.Should().HaveProperty("skyscraperDataInResponse.description", "A tall, continuously habitable building having multiple floors.");

            schemasElement.Should().HaveProperty("spaceDataInPatchRequest.description", "A space within a skyscraper, such as an office, hotel, residential space, or retail space.");
            schemasElement.Should().HaveProperty("spaceDataInPostRequest.description", "A space within a skyscraper, such as an office, hotel, residential space, or retail space.");
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
            schemasElement.Should().HaveProperty("elevatorAttributesInPatchRequest.properties.floorCount.description", "The number of floors this elevator provides access to.");
            schemasElement.Should().HaveProperty("elevatorAttributesInPostRequest.properties.floorCount.description", "The number of floors this elevator provides access to.");
            schemasElement.Should().HaveProperty("elevatorAttributesInResponse.properties.floorCount.description", "The number of floors this elevator provides access to.");

            schemasElement.Should().HaveProperty("skyscraperAttributesInPatchRequest.properties.heightInMeters.description", "The height of this building, in meters.");
            schemasElement.Should().HaveProperty("skyscraperAttributesInPostRequest.properties.heightInMeters.description", "The height of this building, in meters.");
            schemasElement.Should().HaveProperty("skyscraperAttributesInResponse.properties.heightInMeters.description", "The height of this building, in meters.");

            schemasElement.Should().ContainPath("spaceAttributesInPatchRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("floorNumber.description", "The floor number on which this space resides.");
                propertiesElement.Should().HaveProperty("kind.description", "The kind of this space.");
            });

            schemasElement.Should().ContainPath("spaceAttributesInPostRequest.properties").With(propertiesElement =>
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
            schemasElement.Should().HaveProperty("elevatorRelationshipsInPatchRequest.properties.existsIn.description", "The skyscraper this elevator exists in.");
            schemasElement.Should().HaveProperty("elevatorRelationshipsInPostRequest.properties.existsIn.description", "The skyscraper this elevator exists in.");
            schemasElement.Should().HaveProperty("elevatorRelationshipsInResponse.properties.existsIn.description", "The skyscraper this elevator exists in.");

            schemasElement.Should().ContainPath("skyscraperRelationshipsInPatchRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().ContainPath("skyscraperRelationshipsInPostRequest.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().ContainPath("skyscraperRelationshipsInResponse.properties").With(propertiesElement =>
            {
                propertiesElement.Should().HaveProperty("elevator.description", "An optional elevator within this building, providing access to spaces.");
                propertiesElement.Should().HaveProperty("spaces.description", "The spaces within this building.");
            });

            schemasElement.Should().HaveProperty("spaceRelationshipsInPatchRequest.properties.existsIn.description", "The skyscraper this space exists in.");
            schemasElement.Should().HaveProperty("spaceRelationshipsInPostRequest.properties.existsIn.description", "The skyscraper this space exists in.");
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
        document.Should().ContainPath("paths./elevators.post.responses").With(responseElement =>
        {
            responseElement.Should().HaveProperty("403.description", "Client-generated IDs cannot be used at this endpoint.");
        });
    }
}
