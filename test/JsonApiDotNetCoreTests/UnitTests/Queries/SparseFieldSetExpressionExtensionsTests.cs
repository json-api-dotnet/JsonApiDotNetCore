using System.Collections.Immutable;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

public sealed class SparseFieldSetExpressionExtensionsTests
{
    private readonly IResourceGraph _resourceGraph;

    public SparseFieldSetExpressionExtensionsTests()
    {
        var options = new JsonApiOptions();
        _resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Contact, long>().Build();
    }

    [Fact]
    public void Including_preserves_nested_field_elements()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));
        CompoundType contentType = contentAttr.CompoundType!;
        AttrAttribute nameAttr = contentType.GetAttributeByPropertyName(nameof(ContactContent.Name));
        CompoundType nameType = nameAttr.CompoundType!;
        AttrAttribute lastNameAttr = nameType.GetAttributeByPropertyName(nameof(PersonName.LastName));

        var lastNameElement = new SparseFieldSetElementExpression(lastNameAttr);

        var nameElement = new SparseFieldSetElementExpression(nameAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(lastNameAttr, lastNameElement));

        var contentElement = new SparseFieldSetElementExpression(contentAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(nameAttr, nameElement));

        var initialSet =
            new SparseFieldSetExpression(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(contentAttr, contentElement));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Remarks, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(2);
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        updatedSet.Elements.Keys.Should().Contain([
            contentAttr,
            remarksAttr
        ]);

        updatedSet.ToString().Should().Be("content.name.lastName,remarks");
    }

    [Fact]
    public void Excluding_removes_matching_field_and_preserves_nested_chains()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));
        CompoundType contentType = contentAttr.CompoundType!;
        AttrAttribute nameAttr = contentType.GetAttributeByPropertyName(nameof(ContactContent.Name));
        CompoundType nameType = nameAttr.CompoundType!;
        AttrAttribute lastNameAttr = nameType.GetAttributeByPropertyName(nameof(PersonName.LastName));
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        var lastNameElement = new SparseFieldSetElementExpression(lastNameAttr);

        var nameElement = new SparseFieldSetElementExpression(nameAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(lastNameAttr, lastNameElement));

        var contentElement = new SparseFieldSetElementExpression(contentAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(nameAttr, nameElement));

        var remarksElement = new SparseFieldSetElementExpression(remarksAttr);

        var initialSet = new SparseFieldSetExpression(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty
            .Add(contentAttr, contentElement).Add(remarksAttr, remarksElement));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Remarks, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(1);
        updatedSet.Elements.Values.Single().Field.Should().Be(contentAttr);
        updatedSet.ToString().Should().Be("content.name.lastName");
    }

    [Fact]
    public void Excluding_top_level_compound_removes_all_its_nested_chains()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));
        CompoundType contentType = contentAttr.CompoundType!;
        AttrAttribute nameAttr = contentType.GetAttributeByPropertyName(nameof(ContactContent.Name));
        CompoundType nameType = nameAttr.CompoundType!;
        AttrAttribute lastNameAttr = nameType.GetAttributeByPropertyName(nameof(PersonName.LastName));
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        var lastNameElement = new SparseFieldSetElementExpression(lastNameAttr);

        var nameElement = new SparseFieldSetElementExpression(nameAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(lastNameAttr, lastNameElement));

        var contentElement = new SparseFieldSetElementExpression(contentAttr,
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(nameAttr, nameElement));

        var remarksElement = new SparseFieldSetElementExpression(remarksAttr);

        var initialSet = new SparseFieldSetExpression(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty
            .Add(contentAttr, contentElement).Add(remarksAttr, remarksElement));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Content, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(1);
        updatedSet.Elements.Values.Single().Field.Should().Be(remarksAttr);
        updatedSet.ToString().Should().Be("remarks");
    }

    [Fact]
    public void Excluding_leaf_from_whole_compound_expands_siblings_and_excludes_leaf()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));
        var wholeContentElement = new SparseFieldSetElementExpression(contentAttr);

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(contentAttr, wholeContentElement));

        // Act - exclude Content.Name.LastName, which should keep Content.Name.FirstName
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.ToString().Should().Be("content.name.firstName");
    }

    [Fact]
    public void Excluding_all_fields_returns_Empty_singleton()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding(remarksAttr);

        // Assert
        updatedSet.Should().BeSameAs(SparseFieldSetExpression.Empty);
        updatedSet.Elements.Should().BeEmpty();
    }

    [Fact]
    public void Including_whole_compound_when_leaf_already_selected_widens_to_whole_compound()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        SparseFieldSetExpression? initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr))).Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Content, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.ToString().Should().Be("content,remarks");
    }

    [Fact]
    public void Including_leaf_when_whole_compound_already_selected_is_noop()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(contentAttr,
                    new SparseFieldSetElementExpression(contentAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.ToString().Should().Be("content");
    }

    [Fact]
    public void Including_accepts_nested_lambda_chain_and_preserves_it()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(2);
        updatedSet.ToString().Should().Be("content.name.lastName,remarks");
    }

    [Fact]
    public void Excluding_accepts_nested_lambda_chain()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        SparseFieldSetExpression? initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr))).Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(1);
        updatedSet.ToString().Should().Be("remarks");
    }

    [Fact]
    public void Including_accepts_anonymous_type_with_nested_and_top_level_lambdas()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => new
        {
            c.Remarks,
            c.Content!.Name!.LastName
        }, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(2);
        updatedSet.ToString().Should().Be("content.name.lastName,remarks");
    }

    [Fact]
    public void Including_single_ResourceFieldAttribute_adds_field_chain()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));

        var initialSet =
            new SparseFieldSetExpression(
                ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(remarksAttr,
                    new SparseFieldSetElementExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including(contentAttr);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(2);

        updatedSet.Elements.Keys.Should().Contain([
            contentAttr,
            remarksAttr
        ]);

        updatedSet.ToString().Should().Be("content,remarks");
    }

    [Fact]
    public void Excluding_single_ResourceFieldAttribute_removes_field_chain()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));

        var initialSet = new SparseFieldSetExpression(ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty
            .Add(remarksAttr, new SparseFieldSetElementExpression(remarksAttr)).Add(contentAttr, new SparseFieldSetElementExpression(contentAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding(remarksAttr);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet.Elements.Should().HaveCount(1);
        updatedSet.Elements.Values.Single().Field.Should().Be(contentAttr);
        updatedSet.ToString().Should().Be("content");
    }

    [Fact]
    public void Including_or_Excluding_when_null_returns_null()
    {
        SparseFieldSetExpression? nullSet = null;

        SparseFieldSetExpression? includeResult = nullSet.Including<Contact>(c => c.Remarks, _resourceGraph);
        SparseFieldSetExpression? excludeResult = nullSet.Excluding<Contact>(c => c.Remarks, _resourceGraph);

        includeResult.Should().BeNull();
        excludeResult.Should().BeNull();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Contact : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ContactContent? Content { get; set; }

        [Attr]
        public string? Remarks { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ContactContent
    {
        [Attr(IsCompound = true)]
        public PersonName? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PersonName
    {
        [Attr]
        public string? FirstName { get; set; }

        [Attr]
        public string? LastName { get; set; }
    }
}
