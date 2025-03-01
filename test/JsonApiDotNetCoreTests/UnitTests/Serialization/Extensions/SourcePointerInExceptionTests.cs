using System.Net;
using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Extensions;

public sealed class SourcePointerInExceptionTests
{
    private const string RequestBody = """
        {
          "data": {
            "type": "testResources",
            "attributes": {
              "ext-namespace:ext-name": "ignored"
            }
          }
        }
        """;

    [Fact]
    public async Task Adds_source_pointer_to_JsonApiException_thrown_from_JsonConverter()
    {
        // Arrange
        const string? relativeSourcePointer = null;

        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<TestResource, long>().Build();
        var converter = new ThrowingResourceObjectConverter(resourceGraph, relativeSourcePointer);
        var reader = new FakeJsonApiReader(RequestBody, options, converter);
        var httpContext = new DefaultHttpContext();

        // Act
        Func<Task> action = async () => await reader.ReadAsync(httpContext.Request);

        // Assert
        JsonApiException? exception = (await action.Should().ThrowExactlyAsync<JsonApiException>()).Which;

        exception.StackTrace.Should().Contain(nameof(ThrowingResourceObjectConverter));
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Extension error");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
    }

    [Fact]
    public async Task Makes_source_pointer_absolute_in_JsonApiException_thrown_from_JsonConverter()
    {
        // Arrange
        const string relativeSourcePointer = "relative/path";

        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<TestResource, long>().Build();
        var converter = new ThrowingResourceObjectConverter(resourceGraph, relativeSourcePointer);
        var reader = new FakeJsonApiReader(RequestBody, options, converter);
        var httpContext = new DefaultHttpContext();

        // Act
        Func<Task> action = async () => await reader.ReadAsync(httpContext.Request);

        // Assert
        JsonApiException? exception = (await action.Should().ThrowExactlyAsync<JsonApiException>()).Which;

        exception.StackTrace.Should().Contain(nameof(ThrowingResourceObjectConverter));
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Extension error");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relative/path");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TestResource : Identifiable<long>;

    private sealed class ThrowingResourceObjectConverter(IResourceGraph resourceGraph, string? relativeSourcePointer)
        : ResourceObjectConverter(resourceGraph)
    {
        private readonly string? _relativeSourcePointer = relativeSourcePointer;

        private protected override void ValidateExtensionInAttributes(string extensionNamespace, string extensionName, ResourceType resourceType,
            Utf8JsonReader reader)
        {
            var exception = new JsonApiException(new ErrorObject(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Extension error"
            });

            if (_relativeSourcePointer != null)
            {
                exception.Errors[0].Source = new ErrorSource
                {
                    Pointer = _relativeSourcePointer
                };
            }

            CapturedThrow(exception);
        }
    }

    private sealed class FakeJsonApiReader : IJsonApiReader
    {
        private readonly string _requestBody;

        private readonly JsonSerializerOptions _serializerOptions;

        public FakeJsonApiReader(string requestBody, JsonApiOptions options, ResourceObjectConverter converter)
        {
            _requestBody = requestBody;

            _serializerOptions = new JsonSerializerOptions(options.SerializerOptions);
            _serializerOptions.Converters.Add(converter);
        }

        public Task<object?> ReadAsync(HttpRequest httpRequest)
        {
            try
            {
                JsonSerializer.Deserialize<Document>(_requestBody, _serializerOptions);
            }
            catch (NotSupportedException exception) when (exception.HasJsonApiException())
            {
                throw exception.EnrichSourcePointer();
            }

            return Task.FromResult<object?>(null);
        }
    }
}
