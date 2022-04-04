using System.Collections.Immutable;
using System.ComponentModel.Design;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

public sealed class QueryExpressionRewriterTests
{
    private static readonly IResourceFactory ResourceFactory = new ResourceFactory(new ServiceContainer());

    // @formatter:wrap_chained_method_calls chop_always
    // @formatter:keep_existing_linebreaks true

    private static readonly IResourceGraph ResourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
        .Add<Blog, int>()
        .Add<BlogPost, int>()
        .Add<Label, int>()
        .Add<Comment, int>()
        .Add<WebAccount, int>()
        .Add<Human, int>()
        .Add<Man, int>()
        .Add<Woman, int>()
        .Add<AccountPreferences, int>()
        .Add<LoginAttempt, int>()
        .Build();

    // @formatter:wrap_chained_method_calls restore
    // @formatter:keep_existing_linebreaks restore

    [Theory]
    [InlineData("posts", "Include,IncludeElement")]
    [InlineData("posts.comments,owner.loginAttempts", "Include,IncludeElement,IncludeElement,IncludeElement,IncludeElement")]
    public void VisitInclude(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new IncludeParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType, null);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
    }

    [Theory]
    [InlineData("title", "SparseFieldSet")]
    [InlineData("title,posts", "SparseFieldSet")]
    public void VisitSparseFieldSet(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new SparseFieldSetParser();
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType)!;
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
    }

    [Fact]
    public void VisitSparseFieldTable()
    {
        // Arrange
        var parser = new SparseFieldSetParser();

        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();
        ResourceType commentType = ResourceGraph.GetResourceType<Comment>();

        var sparseFieldTable = new Dictionary<ResourceType, SparseFieldSetExpression>
        {
            [blogType] = parser.Parse("title,owner", blogType)!,
            [commentType] = parser.Parse("text,createdAt", commentType)!
        };

        var expression = new SparseFieldTableExpression(sparseFieldTable.ToImmutableDictionary());
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();

        visitedTypeNames.Should().HaveCount(3);
        visitedTypeNames[0].Should().Be("SparseFieldTableExpression");
        visitedTypeNames[1].Should().Be("SparseFieldSetExpression");
        visitedTypeNames[2].Should().Be("SparseFieldSetExpression");
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
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
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
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
    }

    [Theory]
    [InlineData("2", "PaginationQueryStringValue,PaginationElementQueryStringValue")]
    [InlineData("posts:3,2", "PaginationQueryStringValue,PaginationElementQueryStringValue,ResourceFieldChain,PaginationElementQueryStringValue")]
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
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
    }

    [Theory]
    [InlineData("filter", "QueryStringParameterScope,LiteralConstant")]
    [InlineData("filter[posts.comments]", "QueryStringParameterScope,LiteralConstant,ResourceFieldChain")]
    public void VisitParameterScope(string expressionText, string expectedTypes)
    {
        // Arrange
        var parser = new QueryStringParameterScopeParser(FieldChainRequirements.EndsInToMany);
        ResourceType blogType = ResourceGraph.GetResourceType<Blog>();

        QueryExpression expression = parser.Parse(expressionText, blogType);
        var rewriter = new TestableQueryExpressionRewriter();

        // Act
        rewriter.Visit(expression, null);

        // Assert
        List<string> visitedTypeNames = rewriter.ExpressionsVisited.Select(queryExpression => queryExpression.GetType().Name).ToList();
        List<string> expectedTypeNames = expectedTypes.Split(',').Select(type => type + "Expression").ToList();

        visitedTypeNames.Should().ContainInOrder(expectedTypeNames);
        visitedTypeNames.Should().HaveCount(expectedTypeNames.Count);
    }
}
