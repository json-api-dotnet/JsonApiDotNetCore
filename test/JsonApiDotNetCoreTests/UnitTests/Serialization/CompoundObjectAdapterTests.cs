using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Request;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization;

public sealed class CompoundObjectAdapterTests
{
    private static readonly Type[] ResourceTypes =
    [
        typeof(Page),
        typeof(CollectionTypes)
    ];

    [Theory]
    [InlineData("\"text\"")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("[]")]
    public void Cannot_convert_compound_given_as_non_object(string json)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), $$"""{"content":{{json}}}""", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SourcePointer.Should().Be("/data/attributes/content");
        exception.SpecificMessage.Should().StartWith("Failed to convert attribute 'content' with value ");
    }

    [Theory]
    [InlineData("\"text\"")]
    [InlineData("123")]
    [InlineData("false")]
    [InlineData("[]")]
    public void Cannot_convert_nested_compound_given_as_non_object(string json)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), "{\"content\":{\"name\":" + json + "}}", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SourcePointer.Should().Be("/data/attributes/content/name");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("\"text\"")]
    [InlineData("1")]
    public void Cannot_convert_compound_collection_given_as_non_array(string json)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), "{\"content\":{\"phoneNumbers\":" + json + "}}", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SourcePointer.Should().Be("/data/attributes/content/phoneNumbers");
    }

    [Fact]
    public void Cannot_convert_compound_collection_element_given_as_number()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"content":{"phoneNumbers":[{"number":"1"},5]}}""", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SourcePointer.Should().Be("/data/attributes/content/phoneNumbers/1");
        exception.SpecificMessage.Should().StartWith("Failed to convert element with value '5'");
    }

    [Fact]
    public void Can_convert_null_for_compound_collection_and_scalar_collection()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);

        string body = harness.CreateResourceDocument(typeof(Page), """{"content":null,"rulesList":null,"ids":null,"address":null}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Content.Should().BeNull();
        resource.RulesList.Should().BeNull();
        resource.Ids.Should().BeNull();
        resource.Address.Should().BeNull();
    }

    [Fact]
    public void Can_convert_null_for_nested_members()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);

        string body = harness.CreateResourceDocument(typeof(Page), """{"content":{"name":null,"phoneNumbers":null,"tags":null,"title":null}}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Content.Should().NotBeNull();
        resource.Content.Name.Should().BeNull();
        resource.Content.PhoneNumbers.Should().BeNull();
        resource.Content.Tags.Should().BeNull();
        resource.Content.Title.Should().BeNull();
    }

    [Fact]
    public void Can_use_default_or_initializer_for_missing_members()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"content":{"title":"t"}}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Content.Should().NotBeNull();
        resource.Content.Title.Should().Be("t");
        resource.Content.Name.Should().BeNull();
        resource.Content.PhoneNumbers.Should().BeEmpty();
        resource.Content.Count.Should().Be(0);
    }

    [Theory]
    [InlineData("""{"content":{"unknown":1}}""", "/data/attributes/content/unknown", "Attribute 'unknown' does not exist on compound type 'ContentCompound'.")]
    [InlineData("""{"content":{"name":{"middle":"x"}}}""", "/data/attributes/content/name/middle",
        "Attribute 'middle' does not exist on compound type 'NameCompound'.")]
    [InlineData("""{"content":{"phoneNumbers":[{"number":"1"},{"extra":"x"}]}}""", "/data/attributes/content/phoneNumbers/1/extra",
        "Attribute 'extra' does not exist on compound type 'PhoneCompound'.")]
    public void Cannot_convert_unknown_nested_member_when_not_allowed(string attributesJson, string expectedPointer, string expectedDetail)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(options => options.AllowUnknownFieldsInRequestBody = false, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), attributesJson, false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Unknown attribute found.");
        exception.SpecificMessage.Should().Be(expectedDetail);
        exception.SourcePointer.Should().Be(expectedPointer);
    }

    [Fact]
    public void Can_ignore_unknown_nested_members_when_allowed()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(options => options.AllowUnknownFieldsInRequestBody = true, ResourceTypes);

        string body = harness.CreateResourceDocument(typeof(Page), """
        {
          "content": {
            "unknown": 1,
            "title": "t",
            "name": { "first": "f", "middle": "x" },
            "phoneNumbers": [ { "number": "1", "extra": { "a": 1 }, "detail": { "label": "l", "extra": { "note": "n", "other": 2 }, "unknown": [] } } ]
          }
        }
        """, false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Content.Should().NotBeNull();
        resource.Content.Title.Should().Be("t");
        resource.Content.Name.Should().NotBeNull();
        resource.Content.Name.First.Should().Be("f");
        resource.Content.PhoneNumbers.Should().ContainSingle();
        resource.Content.PhoneNumbers[0].Number.Should().Be("1");
        resource.Content.PhoneNumbers[0].Detail.RefShould().NotBeNull().And.Subject.Extra.RefShould().NotBeNull().And.Subject.Note.Should().Be("n");
    }

    [Fact]
    public void Cannot_match_member_names_case_insensitively()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"address":{"ZipCode":"1"}}""", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Unknown attribute found.");
        exception.SourcePointer.Should().Be("/data/attributes/address/ZipCode");
    }

    [Fact]
    public void Can_map_members_by_public_name_including_hyphen()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"address":{"zip-code":"1234","city":"Springfield"}}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Address.Should().NotBeNull();
        resource.Address.ZipCode.Should().Be("1234");
        resource.Address.City.Should().Be("Springfield");
    }

    [Theory]
    [InlineData(WriteOperationKind.CreateResource, """{"rules":{"noCreate":"x"}}""", "Attribute value cannot be assigned when creating resource.",
        "/data/attributes/rules/noCreate")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rules":{"noChange":"x"}}""", "Attribute value cannot be assigned when updating resource.",
        "/data/attributes/rules/noChange")]
    [InlineData(WriteOperationKind.CreateResource, """{"rules":{"readOnly":"x"}}""", "Attribute is read-only.", "/data/attributes/rules/readOnly")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rules":{"readOnly":"x"}}""", "Attribute is read-only.", "/data/attributes/rules/readOnly")]
    [InlineData(WriteOperationKind.CreateResource, """{"rulesList":[{"noCreate":"x"}]}""", "Attribute value cannot be assigned when creating resource.",
        "/data/attributes/rulesList/0/noCreate")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rulesList":[{"noChange":"a"},{"noChange":"x"}]}""",
        "Attribute value cannot be assigned when updating resource.", "/data/attributes/rulesList/0/noChange")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rulesList":[{},{"readOnly":"x"}]}""", "Attribute is read-only.",
        "/data/attributes/rulesList/1/readOnly")]
    public void Cannot_assign_blocked_nested_member(WriteOperationKind writeOperation, string attributesJson, string expectedTitle, string expectedPointer)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), attributesJson, writeOperation == WriteOperationKind.UpdateResource);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, writeOperation);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be(expectedTitle);
        exception.SourcePointer.Should().Be(expectedPointer);
    }

    [Theory]
    [InlineData(WriteOperationKind.CreateResource, """{"rules":{"noChange":"x"}}""")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rules":{"noCreate":"x"}}""")]
    [InlineData(WriteOperationKind.CreateResource, """{"rulesList":[{"noChange":"x"}]}""")]
    [InlineData(WriteOperationKind.UpdateResource, """{"rulesList":[{"noCreate":"x"}]}""")]
    public void Can_assign_nested_member_that_is_blocked_for_other_operation_only(WriteOperationKind writeOperation, string attributesJson)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), attributesJson, writeOperation == WriteOperationKind.UpdateResource);

        // Act
        var resource = harness.ConvertResource<Page>(body, writeOperation);

        // Assert
        resource.Should().NotBeNull();
    }

    [Fact]
    public void Can_report_same_error_for_member_blocked_by_two_rules_at_every_depth()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string topLevelBody = harness.CreateResourceDocument(typeof(Page), """{"readOnlyAndBlocked":"x"}""", false);
        string nestedBody = harness.CreateResourceDocument(typeof(Page), """{"rules":{"readOnlyAndBlocked":"x"}}""", false);

        // Act
        Action topLevelAction = () => harness.ConvertResource<Page>(topLevelBody, WriteOperationKind.CreateResource);
        Action nestedAction = () => harness.ConvertResource<Page>(nestedBody, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException topLevel = topLevelAction.Should().ThrowExactly<ModelConversionException>().Which;
        ModelConversionException nested = nestedAction.Should().ThrowExactly<ModelConversionException>().Which;

        nested.GenericMessage.Should().Be(topLevel.GenericMessage);
    }

    [Theory]
    [InlineData("""{"content":{"count":"abc"}}""", "/data/attributes/content/count")]
    [InlineData("""{"content":{"small":300}}""", "/data/attributes/content/small")]
    [InlineData("""{"content":{"count":null}}""", "/data/attributes/content/count")]
    [InlineData("""{"content":{"count":[1]}}""", "/data/attributes/content/count")]
    [InlineData("""{"content":{"flag":"yes"}}""", "/data/attributes/content/flag")]
    [InlineData("""{"content":{"name":{"first":{"a":1}}}}""", "/data/attributes/content/name/first")]
    [InlineData("""{"content":{"phoneNumbers":[{"number":1}]}}""", "/data/attributes/content/phoneNumbers/0/number")]
    public void Cannot_convert_nested_scalar_of_wrong_kind(string attributesJson, string expectedPointer)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), attributesJson, false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SpecificMessage.Should().StartWith("Failed to convert attribute ");
        exception.SourcePointer.Should().Be(expectedPointer);
    }

    [Theory]
    [InlineData("""{"content":{"tags":["x"]}}""", "/data/attributes/content/tags")]
    [InlineData("""{"ids":["x"]}""", "/data/attributes/ids")]
    [InlineData("""{"nullableNumbers":[1.5]}""", "/data/attributes/nullableNumbers")]
    [InlineData("""{"ids":"not-an-array"}""", "/data/attributes/ids")]
    public void Cannot_convert_scalar_collection_element_of_wrong_kind(string attributesJson, string expectedPointer)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), attributesJson, false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SpecificMessage.Should().StartWith("Failed to convert attribute ");
        exception.SourcePointer.Should().Be(expectedPointer);
    }

    [Fact]
    public void Can_prefix_pointer_with_operation_position_in_atomic_request()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateOperationsDocument(typeof(Page), """{"content":{"phoneNumbers":[{"number":1}]}}""");

        // Act
        Action action = () => harness.ConvertOperations(body);

        // Assert
        ModelConversionException exception = action.Should().ThrowExactly<ModelConversionException>().Which;
        exception.GenericMessage.Should().Be("Incompatible attribute value found.");
        exception.SourcePointer.Should().Be("/atomic:operations/0/data/attributes/content/phoneNumbers/0/number");
    }

    [Fact]
    public void Can_convert_valid_compound_in_atomic_request()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateOperationsDocument(typeof(Page), """{"content":{"title":"t","phoneNumbers":[{"number":"1"}]}}""");

        // Act
        IList<OperationContainer> operations = harness.ConvertOperations(body);

        // Assert
        operations.Should().ContainSingle();
        var resource = (Page)operations[0].Resource;
        resource.Content.RefShould().NotBeNull().And.Subject.PhoneNumbers.Should().ContainSingle();
    }

    [Fact]
    public void Can_create_compound_with_private_parameterless_constructor()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"hidden":{"value":"v"}}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Hidden.Should().NotBeNull();
        resource.Hidden.Value.Should().Be("v");
    }

    [Theory]
    [InlineData(nameof(CollectionTypes.AsList))]
    [InlineData(nameof(CollectionTypes.AsArray))]
    [InlineData(nameof(CollectionTypes.AsSet))]
    [InlineData(nameof(CollectionTypes.AsIList))]
    [InlineData(nameof(CollectionTypes.AsReadOnlyList))]
    [InlineData(nameof(CollectionTypes.AsICollection))]
    [InlineData(nameof(CollectionTypes.AsEnumerable))]
    [InlineData(nameof(CollectionTypes.AsHashSet))]
    public void Can_create_target_collection_type(string propertyName)
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string publicName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        string body = harness.CreateResourceDocument(typeof(CollectionTypes), $$"""{"{{publicName}}":[{"number":"1"},{"number":"2"}]}""", false);

        // Act
        var resource = harness.ConvertResource<CollectionTypes>(body, WriteOperationKind.CreateResource);

        // Assert
        object? value = typeof(CollectionTypes).GetProperty(propertyName)!.GetValue(resource);
        value.Should().NotBeNull();
        value.Should().BeAssignableTo(typeof(CollectionTypes).GetProperty(propertyName)!.PropertyType);

        ((IEnumerable<PhoneCompound>)value!).Select(phone => phone.Number).Should().Contain([
            "1",
            "2"
        ]).And.HaveCount(2);
    }

    [Fact]
    public void Can_assign_init_only_and_private_setter_members()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"setters":{"initOnly":"a","privateSet":"b"}}""", false);

        // Act
        var resource = harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        resource.Setters.Should().NotBeNull();
        resource.Setters.InitOnly.Should().Be("a");
        resource.Setters.PrivateSet.Should().Be("b");
    }

    [Fact]
    public void Cannot_convert_blank_member_name_inside_compound()
    {
        // Arrange
        using var harness = new RequestAdapterHarness(null, ResourceTypes);
        string body = harness.CreateResourceDocument(typeof(Page), """{"content":{"":"x"}}""", false);

        // Act
        Action action = () => harness.ConvertResource<Page>(body, WriteOperationKind.CreateResource);

        // Assert
        action.Should().ThrowExactly<ModelConversionException>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Page : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ContentCompound? Content { get; set; }

        [Attr(IsCompound = true)]
        public AddressCompound? Address { get; set; }

        [Attr(IsCompound = true)]
        public RulesCompound? Rules { get; set; }

        [Attr(IsCompound = true)]
        public List<RulesCompound>? RulesList { get; set; }

        [Attr(IsCompound = true)]
        public HiddenConstructorCompound? Hidden { get; set; }

        [Attr(IsCompound = true)]
        public SettersCompound? Setters { get; set; }

        [Attr]
        public List<Guid>? Ids { get; set; }

        [Attr]
        public List<int?>? NullableNumbers { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
        public string? ReadOnlyAndBlocked => "x";
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CollectionTypes : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public List<PhoneCompound>? AsList { get; set; }

        [Attr(IsCompound = true)]
        public PhoneCompound[]? AsArray { get; set; }

        [Attr(IsCompound = true)]
        public ISet<PhoneCompound>? AsSet { get; set; }

        [Attr(IsCompound = true)]
        public IList<PhoneCompound>? AsIList { get; set; }

        [Attr(IsCompound = true)]
        public IReadOnlyList<PhoneCompound>? AsReadOnlyList { get; set; }

        [Attr(IsCompound = true)]
        public ICollection<PhoneCompound>? AsICollection { get; set; }

        [Attr(IsCompound = true)]
        public IEnumerable<PhoneCompound>? AsEnumerable { get; set; }

        [Attr(IsCompound = true)]
        public HashSet<PhoneCompound>? AsHashSet { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ContentCompound
    {
        [Attr]
        public string? Title { get; set; }

        [Attr(IsCompound = true)]
        public NameCompound? Name { get; set; }

        [Attr(IsCompound = true)]
        public List<PhoneCompound>? PhoneNumbers { get; set; } = [];

        [Attr]
        public List<Guid>? Tags { get; set; }

        [Attr]
        public int Count { get; set; }

        [Attr]
        public byte Small { get; set; }

        [Attr]
        public bool Flag { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class NameCompound
    {
        [Attr]
        public string? First { get; set; }

        [Attr]
        public string? Last { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class PhoneCompound
    {
        [Attr]
        public string? Number { get; set; }

        [Attr(IsCompound = true)]
        public PhoneDetailCompound? Detail { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class PhoneDetailCompound
    {
        [Attr]
        public string? Label { get; set; }

        [Attr(IsCompound = true)]
        public PhoneExtraCompound? Extra { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class PhoneExtraCompound
    {
        [Attr]
        public string? Note { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AddressCompound
    {
        [Attr(PublicName = "zip-code")]
        public string? ZipCode { get; set; }

        [Attr]
        public string? City { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RulesCompound
    {
        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
        public string? NoCreate { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowChange)]
        public string? NoChange { get; set; }

        [Attr]
        public string? ReadOnly { get; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
        public string? ReadOnlyAndBlocked => "x";
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class HiddenConstructorCompound
    {
        [Attr]
        public string? Value { get; set; }

        private HiddenConstructorCompound()
        {
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SettersCompound
    {
        [Attr]
        public string? InitOnly { get; init; }

        [Attr]
        public string? PrivateSet { get; private set; }
    }
}
