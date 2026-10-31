using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

// Query constraints on included collections, secondary endpoints and relationship endpoints, where resource types have compound attributes.
public sealed class CompoundConstraintsTests : IClassFixture<IntegrationTestContext<TestableStartup<ConstraintsDbContext>, ConstraintsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConstraintsDbContext>, ConstraintsDbContext> _testContext;

    public CompoundConstraintsTests(IntegrationTestContext<TestableStartup<ConstraintsDbContext>, ConstraintsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AuthorsController>();
        testContext.UseController<BooksController>();

        testContext.ConfigureServices(services => services.AddResourceDefinition<BookDefinition>());
    }

    [Fact]
    public async Task Can_filter_included_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C");

        string route = $"/authors/{author.StringId}?include=books&filter[books]=equals(title,'B')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("B");
        responseDocument.Included[0].Attributes.Should().ContainKey("details");
    }

    [Fact]
    public async Task Can_sort_included_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C");

        string route = $"/authors/{author.StringId}?include=books&sort[books]=-title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(3);
        responseDocument.Included[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("C");
        responseDocument.Included[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("B");
        responseDocument.Included[2].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("A");
    }

    [Fact]
    public async Task Can_paginate_included_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C");

        string route = $"/authors/{author.StringId}?include=books&sort[books]=title&page[size]=books:2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("A");
        responseDocument.Included[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("B");
    }

    [Fact]
    public async Task Can_filter_sort_and_paginate_included_collection_at_primary_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C", "D");

        string route = "/authors?include=books&filter[books]=not(equals(title,'A'))&sort[books]=-title&page[size]=books:2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(author.StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("D");
        responseDocument.Included[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("C");
    }

    [Fact]
    public async Task Can_filter_sort_and_paginate_secondary_endpoint()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C", "D");

        string route = $"/authors/{author.StringId}/books?filter=not(equals(title,'A'))&sort=-title&page[size]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("D");
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("C");
    }

    [Fact]
    public async Task Can_filter_sort_and_paginate_relationship_endpoint()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C", "D");

        string route = $"/authors/{author.StringId}/relationships/books?filter=not(equals(title,'A'))&sort=-title&page[size]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(author.Books[3].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(author.Books[2].StringId);
    }

    [Fact]
    public async Task Applies_resource_definition_filter_to_included_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C");
        author.Books[1].IsHidden = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Books.Update(author.Books[1]);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/authors/{author.StringId}?include=books&sort[books]=title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("A");
        responseDocument.Included[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("C");
    }

    [Fact]
    public async Task Applies_resource_definition_filter_to_secondary_endpoint()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B", "C");
        author.Books[1].IsHidden = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Books.Update(author.Books[1]);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/authors/{author.StringId}/books?sort=title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("A");
        responseDocument.Data.ManyValue[1].Attributes.Should().ContainKey("title").WhoseValue.Should().Be("C");
    }

    [Fact]
    public async Task Can_select_nested_compound_attribute_in_included_collection()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B");

        string route = $"/authors/{author.StringId}?include=books&fields[authors]=books&fields[books]=details.edition&sort[books]=title";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("details");
        responseDocument.Included[0].Attributes.Should().NotContainKey("title");
    }

    [Fact]
    public async Task Can_include_chain_with_cyclic_dependency()
    {
        // Arrange
        Author author = await SeedAuthorWithBooksAsync("A", "B");

        string route = "/authors?include=books.author.books.author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(author.StringId);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "books" && resource.Id == author.Books[0].StringId);
        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "books" && resource.Id == author.Books[1].StringId);
    }

    private async Task<Author> SeedAuthorWithBooksAsync(params string[] titles)
    {
        var author = new Author
        {
            Name = "Someone",
            Contact = new AuthorContact
            {
                Email = "someone@example.com",
                City = "Amsterdam"
            },
            Books = titles.Select(title => new Book
            {
                Title = title,
                Details = new BookDetails
                {
                    Isbn = $"isbn-{title}",
                    PageCount = 100,
                    Edition = new BookEdition
                    {
                        Number = 2,
                        Year = 2020
                    }
                }
            }).ToList()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Author>();
            await dbContext.ClearTableAsync<Book>();
            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync();
        });

        return author;
    }
}
