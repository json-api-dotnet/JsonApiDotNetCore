using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

public sealed class CompoundResponseModelAdapterTests
{
    [Fact]
    public void Renders_full_nested_structure_in_declared_order_without_fieldset()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, null, null);

        // Assert
        json.Should().BeJson("""
            {
              "title": "T",
              "content": {
                "headline": "H",
                "given-name": "G",
                "nested": {
                  "deep": "D",
                  "count": 3,
                  "flag": true
                },
                "items": null
              },
              "entries": [
                {
                  "name": "one",
                  "rank": 1,
                  "detail": {
                    "info": "i1"
                  }
                },
                {
                  "name": "two",
                  "rank": 2,
                  "detail": null
                }
              ],
              "ids": [],
              "durations": []
            }
            """);
    }

    [Fact]
    public void Prunes_to_selected_nested_leaf()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "content.nested.deep", null);

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "nested": {
                  "deep": "D"
                }
              }
            }
            """);
    }

    [Fact]
    public void Renders_whole_compound_except_view_blocked_members()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "content", null);

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "headline": "H",
                "given-name": "G",
                "nested": {
                  "deep": "D",
                  "count": 3,
                  "flag": true
                },
                "items": null
              }
            }
            """);
    }

    [Fact]
    public void Prunes_each_element_of_compound_collection_the_same_way()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "entries.name,entries.detail.info", null);

        // Assert
        json.Should().BeJson("""
            {
              "entries": [
                {
                  "name": "one",
                  "detail": {
                    "info": "i1"
                  }
                },
                {
                  "name": "two",
                  "detail": null
                }
              ]
            }
            """);
    }

    [Fact]
    public void Renders_null_element_in_compound_collection_as_null()
    {
        // Arrange
        Profile profile = CreateProfile();
        profile.Entries!.Insert(1, null!);

        // Act
        string json = Convert(profile, "entries.name", null);

        // Assert
        json.Should().BeJson("""
            {
              "entries": [
                {
                  "name": "one"
                },
                null,
                {
                  "name": "two"
                }
              ]
            }
            """);
    }

    [Fact]
    public void Keeps_empty_collections()
    {
        // Arrange
        Profile profile = CreateProfile();
        profile.Entries = [];
        profile.Content!.Items = [];

        // Act
        string json = Convert(profile, "entries,ids,content.items", null);

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "items": []
              },
              "entries": [],
              "ids": []
            }
            """);
    }

    [Fact]
    public void Renders_empty_object_when_fieldset_prunes_every_child()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "content.hidden", null);

        // Assert
        json.Should().BeJson("""
            {
              "content": {}
            }
            """);
    }

    [Fact]
    public void Omits_view_blocked_members_at_every_level()
    {
        // Arrange
        Profile profile = CreateProfile();
        profile.Content!.Hidden = "secret1";
        profile.Content.Nested!.Hidden = "secret2";
        profile.Entries![0].Secret = "secret3";
        profile.Entries[0].Detail!.Private = "secret4";

        // Act
        string json = Convert(profile, null, null);

        // Assert
        json.Should().NotContain("secret");
    }

    [Theory]
    [InlineData(JsonIgnoreCondition.Never, """{"deep":null,"count":0,"flag":null}""")]
    [InlineData(JsonIgnoreCondition.WhenWritingNull, """{"count":0}""")]
    [InlineData(JsonIgnoreCondition.WhenWritingDefault, "{}")]
    public void Applies_ignore_condition_to_nested_members(JsonIgnoreCondition condition, string expectedNested)
    {
        // Arrange
        Profile profile = CreateProfile();
        profile.Content!.Nested = new NestedCompound();

        // Act
        string json = Convert(profile, "content.nested", null, options => options.SerializerOptions.DefaultIgnoreCondition = condition);

        // Assert
        using JsonDocument document = JsonDocument.Parse(json);
        string nested = document.RootElement.GetProperty("content").GetProperty("nested").GetRawText();
        nested.Should().BeJson(expectedNested);
    }

    [Fact]
    public void Passes_scalar_collections_through_unchanged()
    {
        // Arrange
        Profile profile = CreateProfile();

        profile.Ids =
        [
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        profile.Durations =
        [
            TimeSpan.FromMinutes(1),
            TimeSpan.FromHours(2)
        ];

        // Act
        IDictionary<string, object?> attributes = ConvertToAttributes(profile, "ids,durations", null);

        // Assert
        attributes["ids"].Should().BeSameAs(profile.Ids);
        attributes["durations"].Should().BeSameAs(profile.Durations);
    }

    [Fact]
    public void Intersects_query_fieldset_with_resource_definition_fieldset()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "content.headline,content.given-name", "content.headline,content.nested.deep");

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "headline": "H"
              }
            }
            """);
    }

    [Fact]
    public void Resource_definition_fieldset_narrows_compound()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, null, "content.headline");

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "headline": "H"
              }
            }
            """);
    }

    [Fact]
    public void Omits_attributes_when_every_attribute_is_pruned()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        ResourceObject resourceObject = ConvertToResourceObject(profile, string.Empty, null);

        // Assert
        resourceObject.Attributes.Should().BeNull();
    }

    [Fact]
    public void Uses_public_name_of_nested_member_as_key()
    {
        // Arrange
        Profile profile = CreateProfile();

        // Act
        string json = Convert(profile, "content.given-name", null);

        // Assert
        json.Should().BeJson("""
            {
              "content": {
                "given-name": "G"
              }
            }
            """);
    }

    private static Profile CreateProfile()
    {
        return new Profile
        {
            Id = 1,
            Title = "T",
            Content = new ProfileContent
            {
                Headline = "H",
                GivenName = "G",
                Nested = new NestedCompound
                {
                    Deep = "D",
                    Count = 3,
                    Flag = true
                }
            },
            Entries =
            [
                new EntryCompound
                {
                    Name = "one",
                    Rank = 1,
                    Detail = new EntryDetailCompound
                    {
                        Info = "i1"
                    }
                },
                new EntryCompound
                {
                    Name = "two",
                    Rank = 2
                }
            ],
            Ids = [],
            Durations = []
        };
    }

    private static string Convert(Profile profile, string? queryFieldSet, string? resourceDefinitionFieldSet, Action<JsonApiOptions>? configureOptions = null)
    {
        IDictionary<string, object?> attributes = ConvertToAttributes(profile, queryFieldSet, resourceDefinitionFieldSet, configureOptions);

        var options = new JsonApiOptions();
        configureOptions?.Invoke(options);

        return JsonSerializer.Serialize(attributes, ((IJsonApiOptions)options).SerializerWriteOptions);
    }

    private static IDictionary<string, object?> ConvertToAttributes(Profile profile, string? queryFieldSet, string? resourceDefinitionFieldSet,
        Action<JsonApiOptions>? configureOptions = null)
    {
        ResourceObject resourceObject = ConvertToResourceObject(profile, queryFieldSet, resourceDefinitionFieldSet, configureOptions);
        return resourceObject.Attributes!;
    }

    // An empty string for queryFieldSet produces an empty sparse fieldset, which excludes all fields.
    private static ResourceObject ConvertToResourceObject(Profile profile, string? queryFieldSet, string? resourceDefinitionFieldSet,
        Action<JsonApiOptions>? configureOptions = null)
    {
        var options = new JsonApiOptions();
        configureOptions?.Invoke(options);

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Profile, long>().Build();
        ResourceType resourceType = resourceGraph.GetResourceType<Profile>();

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = resourceType,
            PrimaryId = profile.StringId
        };

        List<IQueryConstraintProvider> constraintProviders = [];

        if (queryFieldSet != null)
        {
            SparseFieldSetExpression queryExpression = queryFieldSet.Length == 0 ? SparseFieldSetExpression.Empty : CreateFieldSet(resourceType, queryFieldSet);
            constraintProviders.Add(new FakeQueryConstraintProvider(new ResourceTypeExpressionInScope(resourceType, queryExpression)));
        }

        var resourceDefinitionAccessor = new FakeResourceDefinitionAccessor();

        if (resourceDefinitionFieldSet != null)
        {
            SparseFieldSetExpression definitionExpression = CreateFieldSet(resourceType, resourceDefinitionFieldSet);
            resourceDefinitionAccessor.SparseFieldSetCallback = (_, _) => definitionExpression;
        }

        var evaluatedIncludeCache = new EvaluatedIncludeCache(Array.Empty<IQueryConstraintProvider>());
        var sparseFieldSetEvaluator = new SparseFieldSetEvaluator(constraintProviders, resourceDefinitionAccessor);

        var adapter = new ResponseModelAdapter(request, options, new FakeLinkBuilder(), new FakeMetaBuilder(), resourceDefinitionAccessor,
            evaluatedIncludeCache, sparseFieldSetEvaluator, new FakeRequestQueryStringAccessor());

        Document document = adapter.Convert(profile);
        return document.Data.SingleValue!;
    }


    private static IResourceGraph CreateResourceGraph(JsonApiOptions options)
    {
        return new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Profile, long>().Build();
    }

    private static SparseFieldSetExpression CreateFieldSet(ResourceType resourceType, string commaSeparatedChains)
    {
        var root = new FieldSetNode(null);

        foreach (string chain in commaSeparatedChains.Split(','))
        {
            FieldSetNode current = root;
            IAttributeContainer container = resourceType;

            foreach (string segment in chain.Split('.'))
            {
                AttrAttribute attribute = container.GetAttributeByPublicName(segment);
                current = current.GetOrAddChild(attribute);

                if (attribute.CompoundType != null)
                {
                    container = attribute.CompoundType;
                }
            }
        }

        return new SparseFieldSetExpression(root.ToElements());
    }

    private sealed class FieldSetNode(ResourceFieldAttribute? field)
    {
        private readonly Dictionary<ResourceFieldAttribute, FieldSetNode> _children = [];

        public FieldSetNode GetOrAddChild(ResourceFieldAttribute child)
        {
            if (!_children.TryGetValue(child, out FieldSetNode? node))
            {
                node = new FieldSetNode(child);
                _children[child] = node;
            }

            return node;
        }

        public ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> ToElements()
        {
            return _children.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToElement());
        }

        private SparseFieldSetElementExpression ToElement()
        {
            return new SparseFieldSetElementExpression(field!, ToElements());
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Profile : Identifiable<long>
    {
        [Attr]
        public string? Title { get; set; }

        [Attr(IsCompound = true)]
        public ProfileContent? Content { get; set; }

        [Attr(IsCompound = true)]
        public List<EntryCompound>? Entries { get; set; }

        [Attr]
        public List<Guid>? Ids { get; set; }

        [Attr]
        public List<TimeSpan>? Durations { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ProfileContent
    {
        [Attr]
        public string? Headline { get; set; }

        [Attr(PublicName = "given-name")]
        public string? GivenName { get; set; }

        [Attr(IsCompound = true)]
        public NestedCompound? Nested { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Hidden { get; set; }

        [Attr(IsCompound = true)]
        public List<EntryCompound>? Items { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class NestedCompound
    {
        [Attr]
        public string? Deep { get; set; }

        [Attr]
        public int Count { get; set; }

        [Attr]
        public bool? Flag { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Hidden { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class EntryCompound
    {
        [Attr]
        public string? Name { get; set; }

        [Attr]
        public int Rank { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Secret { get; set; }

        [Attr(IsCompound = true)]
        public EntryDetailCompound? Detail { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class EntryDetailCompound
    {
        [Attr]
        public string? Info { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Private { get; set; }
    }
}
