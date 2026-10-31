using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

/// <summary>
/// Parser tests for shapes that the QueryStrings test domain does not have: nested compound collections, scalar collections inside compound collections,
/// generic compound types, derived-only compounds and capability-restricted collections. Uses a small private model.
/// </summary>
public sealed class CompoundShapeParseTests
{
    private readonly JsonApiOptions _options = new()
    {
        EnableLegacyFilterNotation = false
    };

    private readonly IResourceGraph _resourceGraph;
    private readonly JsonApiRequest _request = new();

    public CompoundShapeParseTests()
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        _resourceGraph = new ResourceGraphBuilder(_options, NullLoggerFactory.Instance)
            .Add<Container, long>()
            .Add<Listing, long>()
            .Add<Apartment, long>()
            .Add<House, long>()
            .Add<Parking, long>()
            .Build();

        // @formatter:wrap_chained_method_calls restore
        // @formatter:wrap_before_first_method_call restore
    }

    [Theory]
    // Nested compound collection inside compound collection
    [InlineData("containers", "filter", "has(members,has(phones))", null)]
    [InlineData("containers", "filter", "has(members,has(phones,equals(number,'1')))", null)]
    [InlineData("containers", "filter", "has(members,has(phones,and(equals(number,'1'),not(equals(label,null)))))", null)]
    // Scalar collection inside compound collection
    [InlineData("containers", "filter", "has(members,has(nicknames))", null)]
    [InlineData("containers", "filter", "has(members,has(nicknames,equals(_,'a')))", null)]
    [InlineData("containers", "filter", "has(members,has(nicknames,startsWith(_,'a')))", null)]
    // Count inside the scope of a compound collection
    [InlineData("containers", "filter", "has(members,greaterThan(count(phones),'1'))", null)]
    [InlineData("containers", "filter", "has(members,equals(count(nicknames),'1'))", null)]
    [InlineData("containers", "filter", "has(members,has(phones,equals(count(notes),'1')))", null)]
    // Generic compound type
    [InlineData("containers", "filter", "has(emailAddresses,equals(label,'x'))", null)]
    [InlineData("containers", "filter", "has(emailAddresses,contains(value,'@'))", null)]
    // Compound on a derived type
    [InlineData("apartments", "filter", "equals(building.name,'x')", null)]
    [InlineData("apartments", "filter", "equals(building,null)", null)]
    // isType, with the inner condition evaluated on the derived type
    [InlineData("containers", "filter[listings]", "isType(,apartments,equals(building.name,'x'))", "listings")]
    [InlineData("containers", "filter[listings]", "isType(,apartments,equals(building.floors,null))", "listings")]
    public void Filter_Succeeds(string resourceTypeName, string parameterName, string parameterValue, string? scopeExpected)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateFilterReader(resourceTypeName);

        // Act
        reader.Read(parameterName, parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();

        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    [InlineData("containers", "filter", "has(members,^isType(x,y))", "Function 'isType' can only be used on resource types.")]
    [InlineData("containers", "filter", "has(members,has(phones,^isType(x,y)))", "Function 'isType' can only be used on resource types.")]
    [InlineData("containers", "filter", "has(members,equals(^unknown,'x'))", "Field 'unknown' does not exist on compound type 'Member'.")]
    [InlineData("containers", "filter", "has(members,has(phones,equals(^unknown,'x')))", "Field 'unknown' does not exist on compound type 'Phone'.")]
    [InlineData("containers", "filter", "has(members,has(^nickname))", "Field 'nickname' does not exist on compound type 'Member'.")]
    [InlineData("containers", "filter", "has(members,has(nicknames,equals(^x,'1')))", "Function or _ expected.")]
    [InlineData("containers", "filter", "has(members,has(nicknames,^count(_)))", "Filter function expected.")]
    [InlineData("apartments", "filter", "equals(building.^unknown,'x')", "Field 'unknown' does not exist on compound type 'Building'.")]
    [InlineData("apartments", "filter", "equals(^unknown.premium,'x')", "Field 'unknown' does not exist on resource type 'apartments'.")]
    // Filters (unlike sort) do not look for fields on derived types, except inside isType
    [InlineData("listings", "filter", "equals(^building.name,'x')", "Field 'building' does not exist on resource type 'listings'.")]
    [InlineData("listings", "filter", "equals(^building,null)", "Field 'building' does not exist on resource type 'listings'.")]
    public void Filter_Fails(string resourceTypeName, string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateFilterReader(resourceTypeName);
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
    }

    [Fact(Skip = "KNOWN-ISSUE D8: The type name of a generic compound type is reported as its CLR name (LabeledValue`1) instead of with its type argument")]
    public void Filter_Reports_Generic_Compound_Type_Name_With_Type_Argument()
    {
        // Arrange
        FilterQueryStringParameterReader reader = CreateFilterReader("containers");
        var parameterValueSource = new MarkedText("has(emailAddresses,equals(^unknown,'x'))", '^');

        // Act
        Action action = () => reader.Read("filter", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.Detail.Should().Be($"Field 'unknown' does not exist on compound type 'LabeledValue<String>'. {parameterValueSource}");
    }

    [Theory]
    [InlineData("listings", "building.name", null)]
    [InlineData("listings", "-building.name", null)]
    [InlineData("apartments", "building.name,insurance.premium", null)]
    [InlineData("containers", "count(members)", null)]
    [InlineData("containers", "count(emailAddresses)", null)]
    public void Sort_Succeeds(string resourceTypeName, string parameterValue, string? scopeExpected)
    {
        // Arrange
        SortQueryStringParameterReader reader = CreateSortReader(resourceTypeName);

        // Act
        reader.Read("sort", parameterValue);

        // Assert
        RelationshipChainExpressionInScope[] constraints = reader.GetConstraints().Cast<RelationshipChainExpressionInScope>().ToArray();
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Theory]
    [InlineData("listings", "^insurance.premium", "Field 'insurance' is defined on multiple types that derive from resource type 'listings'.")]
    [InlineData("listings", "building.^unknown", "Field 'unknown' does not exist on compound type 'Building'.")]
    [InlineData("apartments", "^unknown.premium", "Field 'unknown' does not exist on resource type 'apartments'.")]
    public void Sort_Fails(string resourceTypeName, string parameterValue, string errorMessage)
    {
        // Arrange
        SortQueryStringParameterReader reader = CreateSortReader(resourceTypeName);
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => reader.Read("sort", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
    }

    [Fact]
    public void Sort_By_Count_Of_Collection_Without_AllowSort_Fails()
    {
        // Arrange
        SortQueryStringParameterReader reader = CreateSortReader("containers");
        var parameterValueSource = new MarkedText("count(^syncDeviceIds)", '^');

        // Act
        Action action = () => reader.Read("sort", parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.Detail.Should().Be($"Sorting on attribute 'syncDeviceIds' is not allowed. {parameterValueSource}");
    }

    [Theory]
    [InlineData("fields[listings]", "title", "title")]
    [InlineData("fields[apartments]", "building.name", "building.name")]
    [InlineData("fields[apartments]", "building.name,building.floors", "building.floors,building.name")]
    [InlineData("fields[containers]", "members.phones", "members.phones")]
    [InlineData("fields[containers]", "members.phones.number", "members.phones.number")]
    [InlineData("fields[containers]", "members.phones.number,members.phones", "members.phones")]
    public void SparseFieldSet_Succeeds(string parameterName, string parameterValue, string valueExpected)
    {
        // Arrange
        SparseFieldSetQueryStringParameterReader reader = CreateSparseFieldSetReader();

        // Act
        reader.Read(parameterName, parameterValue);

        // Assert
        ResourceTypeExpressionInScope constraint = reader.GetConstraints().Cast<ResourceTypeExpressionInScope>().ToArray().Single();
        constraint.Expression.ToString().Should().Be(valueExpected);
    }

    [Theory]
    [InlineData("fields[listings]", "^building.name", "Field 'building' does not exist on resource type 'listings'.")]
    [InlineData("fields[listings]", "^insurance.premium", "Field 'insurance' does not exist on resource type 'listings'.")]
    [InlineData("fields[containers]", "members.^unknown", "Field 'unknown' does not exist on compound type 'Member'.")]
    [InlineData("fields[containers]", "members.nicknames.^x",
        "Field chain on resource type 'containers' failed to match the pattern: zero or more compound attributes, followed by a field. End of field chain expected.")]
    public void SparseFieldSet_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        SparseFieldSetQueryStringParameterReader reader = CreateSparseFieldSetReader();
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        ErrorObject error = exception.Errors.Should().ContainSingle().Which;
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
    }

    private FilterQueryStringParameterReader CreateFilterReader(string resourceTypeName)
    {
        _request.PrimaryResourceType = _resourceGraph.GetResourceType(resourceTypeName);
        _request.IsCollection = true;

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        return new FilterQueryStringParameterReader(scopeParser, valueParser, _request, _resourceGraph, _options);
    }

    private SortQueryStringParameterReader CreateSortReader(string resourceTypeName)
    {
        _request.PrimaryResourceType = _resourceGraph.GetResourceType(resourceTypeName);
        _request.IsCollection = true;

        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new SortParser();
        return new SortQueryStringParameterReader(scopeParser, valueParser, _request, _resourceGraph);
    }

    private SparseFieldSetQueryStringParameterReader CreateSparseFieldSetReader()
    {
        _request.PrimaryResourceType = _resourceGraph.GetResourceType("containers");
        _request.IsCollection = true;

        var scopeParser = new SparseFieldTypeParser(_resourceGraph);
        var sparseFieldSetParser = new SparseFieldSetParser();
        return new SparseFieldSetQueryStringParameterReader(scopeParser, sparseFieldSetParser, _request, _resourceGraph);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Container : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public List<Member> Members { get; set; } = [];

        [Attr(IsCompound = true)]
        public List<LabeledValue<string>> EmailAddresses { get; set; } = [];

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowSort)]
        public List<string> SyncDeviceIds { get; set; } = [];

        [HasMany]
        public ISet<Listing> Listings { get; set; } = new HashSet<Listing>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Member
    {
        [Attr]
        public string? Name { get; set; }

        [Attr(IsCompound = true)]
        public List<Phone> Phones { get; set; } = [];

        [Attr]
        public List<string> Nicknames { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Phone
    {
        [Attr]
        public string? Number { get; set; }

        [Attr]
        public string? Label { get; set; }

        [Attr]
        public List<string> Notes { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class LabeledValue<T>
    {
        [Attr]
        public string? Label { get; set; }

        [Attr]
        public T? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class Listing : Identifiable<long>
    {
        [Attr]
        public string? Title { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Apartment : Listing
    {
        [Attr(IsCompound = true)]
        public Building? Building { get; set; }

        [Attr(IsCompound = true)]
        public Insurance? Insurance { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class House : Listing
    {
        [Attr(IsCompound = true)]
        public Insurance? Insurance { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Parking : Listing
    {
        [Attr(IsCompound = true)]
        public Insurance? Insurance { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Building
    {
        [Attr]
        public string? Name { get; set; }

        [Attr]
        public int? Floors { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Insurance
    {
        [Attr]
        public decimal Premium { get; set; }
    }
}
