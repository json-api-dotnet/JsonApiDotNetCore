using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Extensions;

public sealed class ResourceObjectConverterTests
{
    private static readonly JsonApiMediaTypeExtension TypeInfoMediaTypeExtension = new("https://www.jsonapi.net/ext/type-info");

    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true
    };

    [Fact]
    public void Permits_request_body_without_extension_usage()
    {
        // Arrange
        TestContext testContext = TestContext.WithoutExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "attributes": {
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              },
              "relationships": {
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """;

        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));

        // Act
        ResourceObject resourceObject = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);

        // Assert
        resourceObject.Attributes.Should().ContainKey("baseValue").WhoseValue.Should().Be("baseAttribute");
        resourceObject.Attributes.Should().ContainKey("derivedValue").WhoseValue.Should().Be("derivedAttribute");

        resourceObject.Relationships.Should().ContainKey("parent").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Type.Should().Be("baseTypes");
            value.Data.SingleValue.Id.Should().Be("1");
        });
    }

    [Fact]
    public void Blocks_request_body_with_extension_in_attributes_when_extension_not_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithoutExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "attributes": {
                "type-info:fail": false,
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              }
            }
            """;

        // Act
        Action action = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));
            _ = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);
        };

        // Assert
        action.Should().ThrowExactly<JsonException>().WithMessage("Unsupported usage of JSON:API extension 'type-info' in attributes.");
    }

    [Fact]
    public void Blocks_request_body_with_extension_in_relationships_when_extension_not_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithoutExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "relationships": {
                "type-info:fail": false,
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """;

        // Act
        Action action = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));
            _ = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);
        };

        // Assert
        action.Should().ThrowExactly<JsonException>().WithMessage("Unsupported usage of JSON:API extension 'type-info' in relationships.");
    }

    [Fact]
    public void Permits_request_body_with_extension_when_extension_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "attributes": {
                "type-info:fail": false,
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              },
              "relationships": {
                "type-info:fail": false,
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """;

        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));

        // Act
        ResourceObject resourceObject = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);

        // Assert
        resourceObject.Attributes.Should().NotBeNull();
        resourceObject.Relationships.Should().NotBeNull();
    }

    [Fact]
    public void Throws_for_request_body_with_extension_in_attributes_when_extension_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "attributes": {
                "type-info:fail": true,
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              }
            }
            """;

        // Act
        Action action = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));
            _ = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);
        };

        // Assert
        JsonApiException? exception = action.Should().ThrowExactly<NotSupportedException>().WithInnerExceptionExactly<JsonApiException>().Which;

        exception.StackTrace.Should().Contain(nameof(ExtensionAwareResourceObjectConverter));
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failure requested from attributes.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("attributes/type-info:fail");
    }

    [Fact]
    public void Throws_for_request_body_with_extension_in_relationships_when_extension_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithExtension;

        const string requestJson = """
            {
              "type": "derivedTypes",
              "relationships": {
                "type-info:fail": true,
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """;

        // Act
        Action action = () =>
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(requestJson));
            _ = testContext.Converter.Read(ref reader, typeof(ResourceObject), testContext.SerializerReadOptions);
        };

        // Assert
        JsonApiException? exception = action.Should().ThrowExactly<NotSupportedException>().WithInnerExceptionExactly<JsonApiException>().Which;

        exception.StackTrace.Should().Contain(nameof(ExtensionAwareResourceObjectConverter));
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failure requested from relationships.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("relationships/type-info:fail");
    }

    [Fact]
    public void Hides_extension_in_response_body_when_extension_not_enabled()
    {
        // Arrange
        TestContext testContext = TestContext.WithoutExtension;

        var resourceObject = new ResourceObject
        {
            Type = "derivedTypes",
            Id = "1",
            Attributes = new Dictionary<string, object?>
            {
                ["baseValue"] = "baseAttribute",
                ["derivedValue"] = "derivedAttribute"
            },
            Relationships = new Dictionary<string, RelationshipObject?>
            {
                ["parent"] = new()
                {
                    Data = new SingleOrManyData<ResourceIdentifierObject>(new ResourceIdentifierObject
                    {
                        Type = "baseTypes",
                        Id = "1"
                    })
                }
            }
        };

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            // Act
            testContext.Converter.Write(writer, resourceObject, testContext.SerializerWriteOptions);
        }

        // Assert
        string responseJson = Encoding.UTF8.GetString(stream.ToArray());

        responseJson.Should().BeJson("""
            {
              "type": "derivedTypes",
              "id": "1",
              "attributes": {
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              },
              "relationships": {
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public void Hides_extension_in_response_body_when_extension_enabled_with_base_type()
    {
        // Arrange
        TestContext testContext = TestContext.WithExtension;

        var resourceObject = new ResourceObject
        {
            Type = "baseTypes",
            Id = "1",
            Attributes = new Dictionary<string, object?>
            {
                ["baseValue"] = "baseAttribute"
            },
            Relationships = new Dictionary<string, RelationshipObject?>
            {
                ["parent"] = new()
                {
                    Data = new SingleOrManyData<ResourceIdentifierObject>(new ResourceIdentifierObject
                    {
                        Type = "baseTypes",
                        Id = "1"
                    })
                }
            }
        };

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            // Act
            testContext.Converter.Write(writer, resourceObject, testContext.SerializerWriteOptions);
        }

        // Assert
        string responseJson = Encoding.UTF8.GetString(stream.ToArray());

        responseJson.Should().BeJson("""
            {
              "type": "baseTypes",
              "id": "1",
              "attributes": {
                "baseValue": "baseAttribute"
              },
              "relationships": {
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public void Writes_extension_in_response_body_when_extension_enabled_with_derived_type()
    {
        // Arrange
        TestContext testContext = TestContext.WithExtension;

        var resourceObject = new ResourceObject
        {
            Type = "derivedTypes",
            Id = "1",
            Attributes = new Dictionary<string, object?>
            {
                ["baseValue"] = "baseAttribute",
                ["derivedValue"] = "derivedAttribute"
            },
            Relationships = new Dictionary<string, RelationshipObject?>
            {
                ["parent"] = new()
                {
                    Data = new SingleOrManyData<ResourceIdentifierObject>(new ResourceIdentifierObject
                    {
                        Type = "baseTypes",
                        Id = "1"
                    })
                }
            }
        };

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            // Act
            testContext.Converter.Write(writer, resourceObject, testContext.SerializerWriteOptions);
        }

        // Assert
        string responseJson = Encoding.UTF8.GetString(stream.ToArray());

        responseJson.Should().BeJson("""
            {
              "type": "derivedTypes",
              "id": "1",
              "attributes": {
                "type-info:baseType": "baseTypes",
                "baseValue": "baseAttribute",
                "derivedValue": "derivedAttribute"
              },
              "relationships": {
                "type-info:baseType": "baseTypes",
                "parent": {
                  "data": {
                    "type": "baseTypes",
                    "id": "1"
                  }
                }
              }
            }
            """);
    }

    private sealed class ExtensionAwareResourceObjectConverter : ResourceObjectConverter
    {
        private const string ExtensionNamespace = "type-info";
        private const string ExtensionName = "fail";

        private readonly IResourceGraph _resourceGraph;
        private readonly JsonApiRequestAccessor _requestAccessor;

        private bool IsTypeInfoExtensionEnabled => _requestAccessor.Request.Extensions.Contains(TypeInfoMediaTypeExtension);

        public ExtensionAwareResourceObjectConverter(IResourceGraph resourceGraph, JsonApiRequestAccessor requestAccessor)
            : base(resourceGraph)
        {
            ArgumentNullException.ThrowIfNull(resourceGraph);
            ArgumentNullException.ThrowIfNull(requestAccessor);

            _resourceGraph = resourceGraph;
            _requestAccessor = requestAccessor;
        }

        private protected override void ValidateExtensionInAttributes(string extensionNamespace, string extensionName, ResourceType resourceType,
            Utf8JsonReader reader)
        {
            if (extensionNamespace == ExtensionNamespace && IsTypeInfoExtensionEnabled && extensionName == ExtensionName)
            {
                if (reader.GetBoolean())
                {
                    CapturedThrow(new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                    {
                        Title = "Failure requested from attributes.",
                        Source = new ErrorSource
                        {
                            Pointer = $"attributes/{ExtensionNamespace}:{ExtensionName}"
                        }
                    }));
                }

                return;
            }

            base.ValidateExtensionInAttributes(extensionNamespace, extensionName, resourceType, reader);
        }

        private protected override void ValidateExtensionInRelationships(string extensionNamespace, string extensionName, ResourceType resourceType,
            Utf8JsonReader reader)
        {
            if (extensionNamespace == ExtensionNamespace && IsTypeInfoExtensionEnabled && extensionName == ExtensionName)
            {
                if (reader.GetBoolean())
                {
                    CapturedThrow(new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                    {
                        Title = "Failure requested from relationships.",
                        Source = new ErrorSource
                        {
                            Pointer = $"relationships/{ExtensionNamespace}:{ExtensionName}"
                        }
                    }));
                }

                return;
            }

            base.ValidateExtensionInRelationships(extensionNamespace, extensionName, resourceType, reader);
        }

        private protected override void WriteExtensionInAttributes(Utf8JsonWriter writer, ResourceObject value)
        {
            WriteBaseType(writer, value);
        }

        private protected override void WriteExtensionInRelationships(Utf8JsonWriter writer, ResourceObject value)
        {
            WriteBaseType(writer, value);
        }

        private void WriteBaseType(Utf8JsonWriter writer, ResourceObject value)
        {
            if (IsTypeInfoExtensionEnabled && value.Type != null)
            {
                ResourceType? resourceType = _resourceGraph.FindResourceType(value.Type);

                if (resourceType is { BaseType: not null })
                {
                    writer.WriteString($"{ExtensionNamespace}:baseType", resourceType.BaseType.PublicName);
                }
            }
        }
    }

    private sealed class JsonApiRequestAccessor
    {
        public IJsonApiRequest Request { get; }

        public JsonApiRequestAccessor(IJsonApiRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Request = request;
        }
    }

    private sealed class TestContext
    {
        public static TestContext WithExtension { get; } = new(true);
        public static TestContext WithoutExtension { get; } = new(false);

        public ExtensionAwareResourceObjectConverter Converter { get; }
        public JsonSerializerOptions SerializerReadOptions { get; }
        public JsonSerializerOptions SerializerWriteOptions { get; }

        private TestContext(bool includeExtension)
        {
            var options = new JsonApiOptions();
            var request = new JsonApiRequest();

            if (includeExtension)
            {
                request.Extensions = new HashSet<JsonApiMediaTypeExtension>([TypeInfoMediaTypeExtension]);
            }

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_before_first_method_call true

            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
                .Add<BaseType, Guid>()
                .Add<DerivedType, Guid>()
                .Build();

            // @formatter:wrap_before_first_method_call restore
            // @formatter:wrap_chained_method_calls restore

            var requestAccessor = new JsonApiRequestAccessor(request);
            Converter = new ExtensionAwareResourceObjectConverter(resourceGraph, requestAccessor);

            options.SerializerOptions.Converters.Add(Converter);
            SerializerReadOptions = ((IJsonApiOptions)options).SerializerReadOptions;
            SerializerWriteOptions = ((IJsonApiOptions)options).SerializerWriteOptions;
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private class BaseType : Identifiable<Guid>
    {
        [Attr]
        public string? BaseValue { get; set; }

        [HasOne]
        public BaseType? Parent { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedType : BaseType
    {
        [Attr]
        public string? DerivedValue { get; set; }
    }
}
