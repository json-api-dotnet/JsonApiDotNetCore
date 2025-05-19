using System.ComponentModel.Design;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class FilterParseTests : BaseParseTests
{
    private readonly FilterQueryStringParameterReader _reader;

    public FilterParseTests()
    {
        Options.EnableLegacyFilterNotation = false;

        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var scopeParser = new QueryStringParameterScopeParser();
        var valueParser = new FilterParser(resourceFactory);
        _reader = new FilterQueryStringParameterReader(scopeParser, valueParser, Request, ResourceGraph, Options);
    }

    [Theory]
    [InlineData("filter", true)]
    [InlineData("filter[title]", true)]
    [InlineData("filters", false)]
    [InlineData("filter[", false)]
    [InlineData("filter]", false)]
    public void Reader_Supports_Parameter_Name(string parameterName, bool expectCanParse)
    {
        // Act
        bool canParse = _reader.CanRead(parameterName);

        // Assert
        canParse.Should().Be(expectCanParse);
    }

    [Theory]
    [InlineData(JsonApiQueryStringParameters.Filter, false)]
    [InlineData(JsonApiQueryStringParameters.All, false)]
    [InlineData(JsonApiQueryStringParameters.None, true)]
    [InlineData(JsonApiQueryStringParameters.Page, true)]
    public void Reader_Is_Enabled(JsonApiQueryStringParameters parametersDisabled, bool expectIsEnabled)
    {
        // Act
        bool isEnabled = _reader.IsEnabled(new DisableQueryStringAttribute(parametersDisabled));

        // Assert
        isEnabled.Should().Be(expectIsEnabled);
    }

    [Theory]
    [InlineData("filter[^", "To-many relationship name expected.")]
    [InlineData("filter[^.", "To-many relationship name expected.")]
    [InlineData("filter[posts.^]", "To-many relationship name expected.")]
    [InlineData("filter[posts.author.^]", "To-many relationship name expected.")]
    [InlineData("filter[^unknown]", "To-many relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("filter[^unknown.other]", "Relationship 'unknown' does not exist on resource type 'blogs'.")]
    [InlineData("filter[posts.^caption]", "To-many relationship 'caption' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts.^author]", "To-many relationship 'author' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts.comments.^unknown]", "To-many relationship 'unknown' does not exist on resource type 'comments'.")]
    [InlineData("filter[posts.comments.^text]", "To-many relationship 'text' does not exist on resource type 'comments'.")]
    [InlineData("filter[posts.comments.^parent]", "To-many relationship 'parent' does not exist on resource type 'comments'.")]
    [InlineData("filter[owner.person.^unknown]", "To-many relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^unknown.other]", "Relationship 'unknown' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^hasBeard]", "To-many relationship 'hasBeard' does not exist on resource type 'humans' or any of its derived types.")]
    [InlineData("filter[owner.person.^wife]", "To-many relationship 'wife' does not exist on resource type 'humans' or any of its derived types.")]
    public void Reader_Read_ParameterName_Fails(string parameterName, string errorMessage)
    {
        // Arrange
        var parameterNameSource = new MarkedText(parameterName, '^');

        // Act
        Action action = () => _reader.Read(parameterNameSource.Text, " ");

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterNameSource.Text);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterNameSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterNameSource.Text);
    }

    [Theory]
    [InlineData("filter[posts]", "equals(author,^'some')", "null expected.")]
    [InlineData("filter[posts]", "lessThan(^some,null)", "Field 'some' does not exist on resource type 'blogPosts'.")]
    [InlineData("filter[posts]", "lessThan(author^,null)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "^ ", "Unexpected whitespace.")]
    [InlineData("filter", "contains(owner.displayName^ ,)", "Unexpected whitespace.")]
    [InlineData("filter", "contains(owner.displayName,^ )", "Unexpected whitespace.")]
    [InlineData("filter", "^some", "Filter function expected.")]
    [InlineData("filter", "equals^", "( expected.")]
    [InlineData("filter", "equals^'", "Unexpected ' outside text.")]
    [InlineData("filter", "equals(^", "Function or field name expected.")]
    [InlineData("filter", "equals(^'1'", "Function or field name expected.")]
    [InlineData("filter", "equals(count(posts),^", "Function, field name or value between quotes expected.")]
    [InlineData("filter", "equals(count(posts),^null)", "Function, field name or value between quotes expected.")]
    [InlineData("filter", "equals(owner.^.displayName,'')", "Function or field name expected.")]
    [InlineData("filter", "equals(owner.displayName.^,'')", "Function or field name expected.")]
    [InlineData("filter", "equals(title,'^)", "' expected.")]
    [InlineData("filter", "equals(title,null^", ") expected.")]
    [InlineData("filter", "equals(^null", "Function or field name expected.")]
    [InlineData("filter", "equals(title,^(", "Function, field name, value between quotes or null expected.")]
    [InlineData("filter", "has(posts,^", "Filter function expected.")]
    [InlineData("filter", "contains^)", "( expected.")]
    [InlineData("filter", "contains(title,'a'^,'b')", ") expected.")]
    [InlineData("filter", "contains(^equals(title,'x'),'a')", "Function that returns type 'String' expected.")]
    [InlineData("filter", "contains(title,^null)", "Value between quotes expected.")]
    [InlineData("filter[posts]", "contains(author^,null)",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "any(^null,'a','b')", "Field name expected.")]
    [InlineData("filter", "any(^'a','b','c')", "Field name expected.")]
    [InlineData("filter", "any(title,'b','c',^)", "Value between quotes expected.")]
    [InlineData("filter", "any(equals(title,'x'),^'b')", "Failed to convert 'b' of type 'String' to type 'Boolean'.")]
    [InlineData("filter", "any(title^)", ", expected.")]
    [InlineData("filter[posts]", "any(author^,'a','b')",
        "Field chain on resource type 'blogPosts' failed to match the pattern: zero or more to-one relationships, followed by an attribute. " +
        "To-one relationship or attribute on resource type 'webAccounts' expected.")]
    [InlineData("filter", "and(^", "Filter function expected.")]
    [InlineData("filter", "or(equals(title,'some'),equals(title,'other')^", ") expected.")]
    [InlineData("filter", "or(equals(title,'some'),equals(title,'other'))^)", "End of expression expected.")]
    [InlineData("filter", "and(equals(title,'some')^", ", expected.")]
    [InlineData("filter", "and(^null", "Filter function expected.")]
    [InlineData("filter", "^expr:equals(caption,'some')", "Filter function expected.")]
    [InlineData("filter", "^expr:Equals(caption,'some')", "Filter function expected.")]
    [InlineData("filter", "isType(^", "Relationship name or , expected.")]
    [InlineData("filter", "isType(,^", "Resource type expected.")]
    [InlineData("filter[posts.contributors]", "isType(,^some)", "Resource type 'some' does not exist or does not derive from 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(,^humans)", "Resource type 'humans' does not exist or does not derive from 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(^some,men)", "Field 'some' does not exist on resource type 'humans'.")]
    [InlineData("filter[posts.contributors]", "isType(father.^some,women)", "Field 'some' does not exist on resource type 'men'.")]
    [InlineData("filter[posts.contributors]", "isType(^children,men)",
        "Field chain on resource type 'humans' failed to match the pattern: one or more to-one relationships. " +
        "To-one relationship on resource type 'humans' expected.")]
    [InlineData("filter[posts.contributors]", "isType(mother.^children,men)",
        "Field chain on resource type 'humans' failed to match the pattern: one or more to-one relationships. " +
        "End of field chain or to-one relationship on resource type 'women' expected.")]
    public void Reader_Read_ParameterValue_Fails(string parameterName, string parameterValue, string errorMessage)
    {
        // Arrange
        var parameterValueSource = new MarkedText(parameterValue, '^');

        // Act
        Action action = () => _reader.Read(parameterName, parameterValueSource.Text);

        // Assert
        InvalidQueryStringParameterException exception = action.Should().ThrowExactly<InvalidQueryStringParameterException>().And;

        exception.ParameterName.Should().Be(parameterName);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{errorMessage} {parameterValueSource}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Theory]
    [InlineData("filter", "equals(title,'Brian O''Quote')", null)]
    [InlineData("filter", "equals(title,'!@#$%^&*()-_=+\"''[]{}<>()/|\\:;.,`~')", null)]
    [InlineData("filter", "equals(title,'')", null)]
    [InlineData("filter", "startsWith(owner.displayName,'GivenName ')", null)]
    [InlineData("filter", "endsWith(owner.displayName,' Surname')", null)]
    [InlineData("filter", "contains(owner.displayName,' ')", null)]
    [InlineData("filter[posts]", "equals(caption,'this, that & more')", "posts")]
    [InlineData("filter[owner.posts]", "equals(caption,'some')", "owner.posts")]
    [InlineData("filter[posts.comments]", "equals(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[owner.person.wife.children]", "not(equals(mother,null))", "owner.person.wife.children")]
    [InlineData("filter", "equals(count(posts),'1')", null)]
    [InlineData("filter", "equals(count(posts),count(owner.posts))", null)]
    [InlineData("filter", "equals(has(posts),'true')", null)]
    [InlineData("filter[posts]", "equals(caption,null)", "posts")]
    [InlineData("filter[posts]", "equals(author,null)", "posts")]
    [InlineData("filter[posts]", "equals(author.userName,author.displayName)", "posts")]
    [InlineData("filter[posts.comments]", "lessThan(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "lessOrEqual(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "greaterThan(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter[posts.comments]", "greaterOrEqual(createdAt,'2000-01-01')", "posts.comments")]
    [InlineData("filter", "has(posts)", null)]
    [InlineData("filter", "has(posts,not(equals(url,null)))", null)]
    [InlineData("filter", "contains(title,'this')", null)]
    [InlineData("filter", "startsWith(title,'this')", null)]
    [InlineData("filter", "endsWith(title,'this')", null)]
    [InlineData("filter", "any(title,'this')", null)]
    [InlineData("filter", "any(title,'that','there','this')", null)]
    [InlineData("filter", "any(equals(title,'x'),'true')", null)]
    [InlineData("filter", "and(contains(title,'sales'),contains(title,'marketing'),contains(title,'advertising'))", null)]
    [InlineData("filter[posts]", "or(and(not(equals(author.userName,null)),not(equals(author.displayName,null))),not(has(comments,startsWith(text,'A'))))",
        "posts")]
    [InlineData("filter", "isType(owner.person,men)", null)]
    [InlineData("filter", "isType(owner.person,men,equals(hasBeard,'true'))", null)]
    [InlineData("filter[posts.contributors]", "isType(,women)", "posts.contributors")]
    [InlineData("filter[posts.contributors]", "isType(,women,equals(maidenName,'Austen'))", "posts.contributors")]
    public void Reader_Read_Succeeds(string parameterName, string parameterValue, string? scopeExpected)
    {
        // Act
        _reader.Read(parameterName, parameterValue);

        IReadOnlyCollection<ExpressionInScope> constraints = _reader.GetConstraints();

        // Assert
        ResourceFieldChainExpression? scope = constraints.Select(expressionInScope => expressionInScope.Scope).Single();
        scope?.ToString().Should().Be(scopeExpected);

        QueryExpression value = constraints.Select(expressionInScope => expressionInScope.Expression).Single();
        value.ToString().Should().Be(parameterValue);
    }

    [Fact]
    public void Throws_When_ResourceType_Scope_Not_Disposed()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var parser = new NotDisposingFilterParser(resourceFactory);

        // Act
        Action action = () => parser.Parse("equals(title,'some')", Request.PrimaryResourceType!);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("There is still a resource type in scope after parsing has completed. " +
            "Verify that Dispose() is called on all return values of InScopeOfContainer().");
    }

    [Fact]
    public void Throws_When_No_ResourceType_In_Scope()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var resourceFactory = new ResourceFactory(serviceProvider);
        var parser = new ResourceTypeAccessingFilterParser(resourceFactory);

        // Act
        Action action = () => parser.Parse("equals(title,'some')", Request.PrimaryResourceType!);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("No resource type is currently in scope. Call Parse() first.");
    }

    private sealed class NotDisposingFilterParser(IResourceFactory resourceFactory)
        : FilterParser(resourceFactory)
    {
        protected override FilterExpression ParseFilter()
        {
            // Forgot to dispose the return value.
            _ = InScopeOfContainer(ContainerInScope);

            return base.ParseFilter();
        }
    }

    private sealed class ResourceTypeAccessingFilterParser(IResourceFactory resourceFactory)
        : FilterParser(resourceFactory)
    {
        protected override void Tokenize(string source)
        {
            // There is no resource type in scope yet.
            _ = ContainerInScope;

            base.Tokenize(source);
        }
    }
}
