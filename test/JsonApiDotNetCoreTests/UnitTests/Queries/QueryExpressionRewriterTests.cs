using System.Collections.Immutable;
using System.ComponentModel.Design;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

public sealed class QueryExpressionRewriterTests
{
    private static readonly IResourceFactory ResourceFactory = new ResourceFactory(new ServiceContainer());

    // @formatter:wrap_chained_method_calls chop_always
    // @formatter:wrap_before_first_method_call true

    private static readonly IResourceGraph ResourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
        .Add<Blog, long>()
        .Add<BlogPost, long>()
        .Add<Label, long>()
        .Add<Comment, long>()
        .Add<WebAccount, long>()
        .Add<Human, long>()
        .Add<Man, long>()
        .Add<Woman, long>()
        .Add<AccountPreferences, long>()
        .Add<LoginAttempt, long>()
        .Add<Calendar, long>()
        .Add<Appointment, long>()
        .Add<Reminder, long>()
        .Build();

    // @formatter:wrap_chained_method_calls restore
    // @formatter:wrap_before_first_method_call restore

    [Theory]
    [InlineData("posts", "Include,IncludeElement")]
    [InlineData("posts.comments,owner.loginAttempts", "Include,IncludeElement,IncludeElement,IncludeElement,IncludeElement")]
    public void VisitInclude(string expressionText, string expectedTypes)
    {
        // Arrange
        var options = new JsonApiOptions();
        var parser = new IncludeParser(options);
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Theory]
    [InlineData("title", "SparseFieldSet,SparseFieldSetElement")]
    [InlineData("title,posts", "SparseFieldSet,SparseFieldSetElement,SparseFieldSetElement")]
    public void VisitSparseFieldSet(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new SparseFieldSetParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Theory]
    [InlineData("location", "SparseFieldSet,SparseFieldSetElement")]
    [InlineData("location.name,location.room", "SparseFieldSet,SparseFieldSetElement,SparseFieldSetElement,SparseFieldSetElement")]
    [InlineData("location.address.city,title", "SparseFieldSet,SparseFieldSetElement,SparseFieldSetElement,SparseFieldSetElement,SparseFieldSetElement")]
    public void VisitSparseFieldSetElement_with_children(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new SparseFieldSetParser();
        ResourceType appointmentType = ResourceGraph.GetResourceType<Appointment>();

        QueryExpression expression = parser.Parse(expressionText, appointmentType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Fact]
    public void VisitScalarCollectionElementValue()
    {
        // Arrange
        var parser = new FilterParser(ResourceFactory);
        ResourceType appointmentType = ResourceGraph.GetResourceType<Appointment>();

        QueryExpression expression = parser.Parse("has(keywords,equals(_,'x'))", appointmentType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();

        visitedTypeNames.Should().ContainInOrder("HasExpression", "ResourceFieldChainExpression", "ComparisonExpression",
            "ScalarCollectionElementValueExpression", "LiteralConstantExpression");

        visitedTypeNames.Should().HaveCount(5);
    }

    [Fact(Skip = "KNOWN-ISSUE D13: A rewriter that removes all children of a compound element widens the selection to the whole compound")]
    public void Rewriter_removing_all_children_does_not_widen_selection()
    {
        // Arrange
        var parser = new SparseFieldSetParser();
        ResourceType appointmentType = ResourceGraph.GetResourceType<Appointment>();

        QueryExpression expression = parser.Parse("location.name,location.room,title", appointmentType);
        var rewriter = new RemoveLocationChildrenRewriter();

        // Act
        QueryExpression? result = rewriter.Visit(expression, null);

        // Assert
        var sparseFieldSet = result.Should().BeOfType<SparseFieldSetExpression>().Subject;
        sparseFieldSet.ToString().Should().Be("title");
    }

    [Theory]
    [InlineData("any(userName,'A','B')", "Any,ResourceFieldChain,LiteralConstant,LiteralConstant")]
    [InlineData("equals(userName,null)", "Comparison,ResourceFieldChain,NullConstant")]
    [InlineData("has(loginAttempts)", "Has,ResourceFieldChain")]
    [InlineData("has(loginAttempts,equals(isSucceeded,'true'))", "Has,ResourceFieldChain,Comparison,ResourceFieldChain,LiteralConstant")]
    [InlineData("isType(person,men)", "IsType,ResourceFieldChain")]
    [InlineData("isType(person,men,greaterThan(age,'18'))", "IsType,ResourceFieldChain,Comparison,ResourceFieldChain,LiteralConstant")]
    [InlineData("and(equals(userName,null),has(loginAttempts))", "Logical,Comparison,ResourceFieldChain,NullConstant,Has,ResourceFieldChain")]
    [InlineData("startsWith(userName,'A')", "MatchText,ResourceFieldChain,LiteralConstant")]
    [InlineData("not(equals(count(loginAttempts),'1'))", "Not,Comparison,Count,ResourceFieldChain,LiteralConstant")]
    public void VisitFilter(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new FilterParser(ResourceFactory);
        ResourceType webAccountType = ResourceGraph.GetResourceType<WebAccount>();

        QueryExpression expression = parser.Parse(expressionText, webAccountType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Theory]
    [InlineData("title", "Sort,SortElement,ResourceFieldChain")]
    [InlineData("title,-platformName", "Sort,SortElement,ResourceFieldChain,SortElement,ResourceFieldChain")]
    [InlineData("count(posts)", "Sort,SortElement,Count,ResourceFieldChain")]
    public void VisitSort(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new SortParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Theory]
    [InlineData("2", "PaginationQueryStringValue,PaginationElementQueryStringValue")]
    [InlineData("posts:3,2", "PaginationQueryStringValue,PaginationElementQueryStringValue,Include,IncludeElement,PaginationElementQueryStringValue")]
    public void VisitPagination(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new PaginationParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    [Theory]
    [InlineData("filter", "QueryStringParameterScope,LiteralConstant")]
    [InlineData("filter[posts.comments]", "QueryStringParameterScope,LiteralConstant,Include,IncludeElement,IncludeElement")]
    public void VisitParameterScope(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new QueryStringParameterScopeParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);

        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        string[] visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToArray();
        string[] expectedTypeNames = expectedTypes.Split(',').Select(type => $"{type}Expression").ToArray();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Length);
    }

    private sealed class RemoveLocationChildrenRewriter : QueryExpressionRewriter<object?>
    {
        protected override IImmutableDictionary<TKey, TExpression> VisitDictionary<TKey, TExpression>(IImmutableDictionary<TKey, TExpression> elements,
            object? argument)
        {
            // Removes all children of the compound attribute, but leaves the top-level fields alone.
            IImmutableDictionary<TKey, TExpression> remaining = elements.RemoveRange(elements.Keys.Where(key =>
                key is AttrAttribute { Property.DeclaringType: { } declaringType } && declaringType == typeof(AppointmentLocation)));

            return base.VisitDictionary(remaining, argument);
        }
    }
}
