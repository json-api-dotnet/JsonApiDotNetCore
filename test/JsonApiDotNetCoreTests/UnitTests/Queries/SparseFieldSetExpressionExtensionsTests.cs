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
        _resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<Contact, long>()
            .Build();
    }

    [Fact]
    public void Including_preserves_nested_field_chains()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute contentAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Content));
        CompoundType contentType = contentAttr.CompoundType!;
        AttrAttribute nameAttr = contentType.GetAttributeByPropertyName(nameof(ContactContent.Name));
        CompoundType nameType = nameAttr.CompoundType!;
        AttrAttribute lastNameAttr = nameType.GetAttributeByPropertyName(nameof(PersonName.LastName));

        var nestedChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(contentAttr, nameAttr, lastNameAttr));
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(nestedChain));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Remarks, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(2);
        updatedSet.FieldChains.Should().Contain(nestedChain);
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        updatedSet.FieldChains.Should().Contain(new ResourceFieldChainExpression(remarksAttr));
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

        var nestedChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(contentAttr, nameAttr, lastNameAttr));
        var remarksChain = new ResourceFieldChainExpression(remarksAttr);
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(nestedChain, remarksChain));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Remarks, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(1);
        updatedSet.FieldChains.Should().Contain(nestedChain);
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

        var nestedChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(contentAttr, nameAttr, lastNameAttr));
        var remarksChain = new ResourceFieldChainExpression(remarksAttr);
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(nestedChain, remarksChain));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Content, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(1);
        updatedSet.FieldChains.Should().Contain(remarksChain);
        updatedSet.ToString().Should().Be("remarks");
    }

    [Fact]
    public void Including_accepts_nested_lambda_chain_and_preserves_it()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(new ResourceFieldChainExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(2);
        updatedSet.ToString().Should().Be("content.name.lastName,remarks");
    }

    [Fact]
    public void Excluding_accepts_nested_lambda_chain()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(new ResourceFieldChainExpression(remarksAttr)))
            .Including<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Excluding<Contact>(c => c.Content!.Name!.LastName, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(1);
        updatedSet.ToString().Should().Be("remarks");
    }

    [Fact]
    public void Including_accepts_anonymous_type_with_nested_and_top_level_lambdas()
    {
        // Arrange
        ResourceType contactType = _resourceGraph.GetResourceType<Contact>();
        AttrAttribute remarksAttr = contactType.GetAttributeByPropertyName(nameof(Contact.Remarks));
        var initialSet = new SparseFieldSetExpression(ImmutableHashSet.Create(new ResourceFieldChainExpression(remarksAttr)));

        // Act
        SparseFieldSetExpression? updatedSet = initialSet.Including<Contact>(c => new { c.Remarks, c.Content!.Name!.LastName }, _resourceGraph);

        // Assert
        updatedSet.Should().NotBeNull();
        updatedSet!.FieldChains.Should().HaveCount(2);
        updatedSet.ToString().Should().Be("content.name.lastName,remarks");
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
        public string? LastName { get; set; }
    }
}
