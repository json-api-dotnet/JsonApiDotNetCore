using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesDisabled;

public sealed class RequestTests
{
    private readonly NullableReferenceTypesDisabledFaker _fakers = new();

    [Fact]
    public async Task Can_clear_reference_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Name = null;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument,
            chicken => chicken.Name))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("name").With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.Null));
        });
    }

    [Fact]
    public async Task Can_exclude_reference_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Name = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("name");
        });
    }

    [Fact]
    public async Task Cannot_clear_required_reference_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.NameOfCurrentFarm = default;

        // Act
        Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'nameOfCurrentFarm'. Property requires a value. Path 'data.attributes'.");
    }

    [Fact]
    public async Task Cannot_exclude_required_reference_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.NameOfCurrentFarm = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'nameOfCurrentFarm' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_set_default_value_to_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Age = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("age").With(attribute => attribute.ShouldBeInteger(0));
        });
    }

    [Fact]
    public async Task Can_exclude_value_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Age = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("age");
        });
    }

    [Fact]
    public async Task Can_set_default_value_to_required_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Weight = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("weight").With(attribute => attribute.ShouldBeInteger(0));
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_value_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Weight = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'weight' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_clear_nullable_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.TimeAtCurrentFarmInDays = null;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument,
            chicken => chicken.TimeAtCurrentFarmInDays))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("timeAtCurrentFarmInDays").With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.Null));
        });
    }

    [Fact]
    public async Task Can_exclude_nullable_value_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.TimeAtCurrentFarmInDays = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("timeAtCurrentFarmInDays");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_nullable_value_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.HasProducedEggs = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'hasProducedEggs' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_set_default_value_to_required_nullable_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPostRequestDocument requestDocument = _fakers.ChickenPostRequestDocument.Generate();
        requestDocument.Data.Attributes.HasProducedEggs = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("hasProducedEggs").With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.False));
        });
    }

    [Fact]
    public async Task Can_clear_has_one_relationship()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestChicken.Data = null;

        await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/henHouses");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships.oldestChicken.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Fact]
    public async Task Can_exclude_has_one_relationship_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestChicken = default!;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/henHouses");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("oldestChicken");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_has_one_relationship_without_document_registration_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FirstChicken = default!;

        // Act
        Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'firstChicken'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Cannot_exclude_required_has_one_relationship_with_document_registration_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FirstChicken = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<HenHousePostRequestDocument, HenHouseRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'firstChicken' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Can_exclude_has_many_relationship_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.AllChickens = default!;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/henHouses");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("allChickens");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_has_many_relationship_with_document_registration_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.ChickensReadyForLaying = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<HenHousePostRequestDocument, HenHouseRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'chickensReadyForLaying' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_required_has_many_relationship_without_document_registration_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePostRequestDocument requestDocument = _fakers.HenHousePostRequestDocument.Generate();
        requestDocument.Data.Relationships.ChickensReadyForLaying = default!;

        // Act
        Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'chickensReadyForLaying'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Cannot_exclude_id_when_performing_PATCH()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPatchRequestDocument requestDocument = _fakers.ChickenPatchRequestDocument.Generate();
        requestDocument.Data.Id = default!;

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PatchChickenAsync(999, requestDocument));

        // Assert
        await action.Should().ThrowAsync<JsonSerializationException>();
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Attributes_required_in_POST_request_are_not_required_in_PATCH_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        ChickenPatchRequestDocument requestDocument = _fakers.ChickenPatchRequestDocument.Generate();
        requestDocument.Data.Attributes.NameOfCurrentFarm = default!;
        requestDocument.Data.Attributes.Weight = default!;
        requestDocument.Data.Attributes.HasProducedEggs = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPatchRequestDocument, ChickenAttributesInPatchRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PatchChickenAsync(int.Parse(requestDocument.Data.Id), requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"http://localhost/chickens/{int.Parse(requestDocument.Data.Id)}");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("nameOfCurrentFarm");
            attributesObject.ShouldNotContainPath("weight");
            attributesObject.ShouldNotContainPath("hasProducedMilk");
        });
    }

    [Fact]
    public async Task Relationships_required_in_POST_request_are_not_required_in_PATCH_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHousePatchRequestDocument requestDocument = _fakers.HenHousePatchRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestChicken = default!;
        requestDocument.Data.Relationships.FirstChicken = default!;
        requestDocument.Data.Relationships.ChickensReadyForLaying = default!;
        requestDocument.Data.Relationships.AllChickens = default!;

        await ApiResponse.TranslateAsync(async () => await apiClient.PatchHenHouseAsync(int.Parse(requestDocument.Data.Id), requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"http://localhost/henHouses/{int.Parse(requestDocument.Data.Id)}");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("oldestChicken");
            relationshipsObject.ShouldNotContainPath("firstChicken");
            relationshipsObject.ShouldNotContainPath("favoriteChicken");
            relationshipsObject.ShouldNotContainPath("allChickens");
        });
    }
}
