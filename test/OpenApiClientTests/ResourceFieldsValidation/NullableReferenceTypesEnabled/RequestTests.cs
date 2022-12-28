using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesEnabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldsValidation.NullableReferenceTypesEnabled;

public sealed class RequestTests
{
    private readonly NullableReferenceTypesEnabledFaker _fakers = new();

    [Fact]
    public async Task Cannot_exclude_non_nullable_reference_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Name = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'name' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_required_non_nullable_reference_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.NameOfCurrentFarm = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'nameOfCurrentFarm' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_exclude_nullable_reference_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.NameOfPreviousFarm = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("nameOfPreviousFarm");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_nullable_reference_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Nickname = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'nickname' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_set_default_value_to_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Age = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
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
    public async Task Can_exclude_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Age = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
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
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Weight = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
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
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.Weight = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

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
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.TimeAtCurrentFarmInDays = null;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument,
            cow => cow.TimeAtCurrentFarmInDays))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
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
    public async Task Can_exclude_nullable_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.TimeAtCurrentFarmInDays = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
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
    public async Task Can_set_default_value_to_required_nullable_value_type_attribute()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.HasProducedMilk = default;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cows");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath("hasProducedMilk").With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.False));
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_nullable_value_type_attribute_in_POST_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPostRequestDocument requestDocument = _fakers.CowPostRequestDocument.Generate();
        requestDocument.Data.Attributes.HasProducedMilk = default;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'hasProducedMilk' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_has_one_relationship_in_POST_request_with_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestCow = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowStablePostRequestDocument, CowStableRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'oldestCow' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_has_one_relationship_in_POST_request_without_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestCow = default!;

        // Act
        Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'oldestCow'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Cannot_exclude_required_has_one_relationship_in_POST_request_with_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FirstCow = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowStablePostRequestDocument, CowStableRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'firstCow' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_required_has_one_relationship_in_POST_request_without_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FirstCow = default!;

        // Act
        Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'firstCow'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Can_clear_nullable_has_one_relationship()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.AlbinoCow.Data = null;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cowStables");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships.albinoCow.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Fact]
    public async Task Can_exclude_nullable_has_one_relationship()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.AlbinoCow = default!;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cowStables");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("albinoCow");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_nullable_has_one_relationship_in_POST_request_with_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FavoriteCow = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowStablePostRequestDocument, CowStableRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'favoriteCow' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_required_nullable_has_one_relationship_in_POST_request_without_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.FavoriteCow = default!;

        // Act
        Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'favoriteCow'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Can_exclude_has_many_relationship()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.CowsReadyForMilking = default!;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be("http://localhost/cowStables");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("cowsReadyForMilking");
        });
    }

    [Fact]
    public async Task Cannot_exclude_required_has_many_relationship_in_POST_request_with_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.AllCows = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowStablePostRequestDocument, CowStableRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be("Ignored property 'allCows' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Fact]
    public async Task Cannot_exclude_required_has_many_relationship_in_POST_request_without_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePostRequestDocument requestDocument = _fakers.CowStablePostRequestDocument.Generate();
        requestDocument.Data.Relationships.AllCows = default!;

        // Act
        Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'allCows'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Cannot_exclude_id_when_performing_PATCH()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPatchRequestDocument requestDocument = _fakers.CowPatchRequestDocument.Generate();
        requestDocument.Data.Id = default!;

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PatchCowAsync(999, requestDocument));

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
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowPatchRequestDocument requestDocument = _fakers.CowPatchRequestDocument.Generate();
        requestDocument.Data.Attributes.Name = default!;
        requestDocument.Data.Attributes.NameOfCurrentFarm = default!;
        requestDocument.Data.Attributes.Nickname = default!;
        requestDocument.Data.Attributes.Weight = default!;
        requestDocument.Data.Attributes.HasProducedMilk = default!;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowPatchRequestDocument, CowAttributesInPatchRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PatchCowAsync(int.Parse(requestDocument.Data.Id), requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"http://localhost/cows/{int.Parse(requestDocument.Data.Id)}");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath("name");
            attributesObject.ShouldNotContainPath("nameOfCurrentFarm");
            attributesObject.ShouldNotContainPath("nickname");
            attributesObject.ShouldNotContainPath("weight");
            attributesObject.ShouldNotContainPath("hasProducedMilk");
        });
    }

    [Fact]
    public async Task Relationships_required_in_POST_request_are_not_required_in_PATCH_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStablePatchRequestDocument requestDocument = _fakers.CowStablePatchRequestDocument.Generate();
        requestDocument.Data.Relationships.OldestCow = default!;
        requestDocument.Data.Relationships.FirstCow = default!;
        requestDocument.Data.Relationships.FavoriteCow = default!;
        requestDocument.Data.Relationships.AllCows = default!;

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchCowStableAsync(int.Parse(requestDocument.Data.Id), requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"http://localhost/cowStables/{int.Parse(requestDocument.Data.Id)}");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath("oldestCow");
            relationshipsObject.ShouldNotContainPath("firstCow");
            relationshipsObject.ShouldNotContainPath("favoriteCow");
            relationshipsObject.ShouldNotContainPath("allCows");
        });
    }
}
