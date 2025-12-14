using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

public sealed class DecryptFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<DecryptDbContext>, DecryptDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<DecryptDbContext>, DecryptDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public DecryptFilterTests(IntegrationTestContext<TestableStartup<DecryptDbContext>, DecryptDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, DecryptFilterParser>();
            services.AddTransient<IWhereClauseBuilder, DecryptWhereClauseBuilder>();
        });
    }

    [Fact]
    public async Task Can_filter_on_encrypted_column_at_primary_endpoint()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);

        blogs[0].Title = Convert.ToBase64String("something-else"u8);
        blogs[1].Title = Convert.ToBase64String("two"u8);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.DeclareDecryptFunctionAsync();
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=any(decrypt(title),'one','two','three')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_encrypted_column_in_compound_expression_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(4);

        blog.Posts[0].Caption = Convert.ToBase64String("the-needle-in-the-haystack"u8);
        blog.Posts[0].Url = Convert.ToBase64String("https://www.domain.org"u8);

        blog.Posts[1].Caption = Convert.ToBase64String("the-needle-in-the-haystack"u8);
        blog.Posts[1].Url = Convert.ToBase64String("https://www.domain.com"u8);

        blog.Posts[2].Caption = Convert.ToBase64String("something-else"u8);
        blog.Posts[2].Url = Convert.ToBase64String("https://www.domain.org"u8);

        blog.Posts[3].Caption = Convert.ToBase64String("something-else"u8);
        blog.Posts[3].Url = Convert.ToBase64String("https://www.domain.com"u8);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.DeclareDecryptFunctionAsync();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?filter=and(contains(decrypt(caption),'needle'),not(endsWith(decrypt(url),'.org')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_encrypted_column_in_included_resources()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Title = Convert.ToBase64String("one"u8);
        blogs[1].Title = Convert.ToBase64String("two"u8);

        blogs[1].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Posts[0].Caption = Convert.ToBase64String("first-value"u8);
        blogs[1].Posts[1].Caption = Convert.ToBase64String("second-value"u8);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.DeclareDecryptFunctionAsync();
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=equals(decrypt(title),'two')&include=posts&filter[posts]=startsWith(decrypt(caption),'second')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blogs[1].Posts[1].StringId);
    }
}
