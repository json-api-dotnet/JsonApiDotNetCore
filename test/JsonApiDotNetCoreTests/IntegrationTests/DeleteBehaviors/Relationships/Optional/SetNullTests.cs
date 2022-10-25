using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.DeleteBehaviors.Relationships.Optional;

public sealed class SetNullTests : IClassFixture<IntegrationTestContext<TestableStartup<BloggingDbContext>, BloggingDbContext>>
{
    private const string OwnerName = "Jack";
    private const string AuthorName = "Jull";

    private readonly IntegrationTestContext<TestableStartup<BloggingDbContext>, BloggingDbContext> _testContext;

    public SetNullTests(IntegrationTestContext<TestableStartup<BloggingDbContext>, BloggingDbContext> testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public async Task Deleting_a_blog_will_cause_the_blog_in_all_the_related_posts_to_become_null()
    {
        // Arrange
        await StoreTestDataAsync();

        // Act
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Blog blog = await dbContext.Blogs.SingleAsync();
            dbContext.Remove(blog);

            await dbContext.SaveChangesAsync();
        });

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Should().BeEmpty();
            dbContext.Posts.Should().ContainSingle(post => post.Blog == null);
            dbContext.People.Should().HaveCount(2);
            await Task.Yield();
        });
    }

    [Fact]
    public async Task Deleting_the_author_of_posts_will_cause_the_author_of_authored_posts_to_become_null()
    {
        // Arrange
        await StoreTestDataAsync();

        // Act
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person author = await dbContext.People.SingleAsync(person => person.Name == AuthorName);
            dbContext.Remove(author);

            await dbContext.SaveChangesAsync();
        });

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Should().HaveCount(1);
            dbContext.Posts.Should().ContainSingle(post => post.Author == null);
            dbContext.People.Should().ContainSingle(person => person.Name == OwnerName);
            await Task.Yield();
        });
    }

    [Fact]
    public async Task Deleting_the_owner_of_a_blog_will_cause_the_owner_of_blog_to_become_null()
    {
        // Arrange
        await StoreTestDataAsync();

        // Act
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person owner = await dbContext.People.SingleAsync(person => person.Name == OwnerName);
            dbContext.Remove(owner);

            await dbContext.SaveChangesAsync();
        });

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Should().ContainSingle(blog => blog.Owner == null);
            dbContext.Posts.Should().HaveCount(1);
            dbContext.People.Should().ContainSingle(person => person.Name == AuthorName);
            await Task.Yield();
        });
    }

    private async Task StoreTestDataAsync()
    {
        Post newPost = CreateTestData();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            await dbContext.ClearTableAsync<Post>();
            await dbContext.ClearTableAsync<Person>();

            dbContext.Posts.Add(newPost);
            await dbContext.SaveChangesAsync();
        });
    }

    private static Post CreateTestData()
    {
        return new Post
        {
            Title = "Cascading Deletes",
            Content = "...",
            Blog = new Blog
            {
                Name = "EF Core",
                Owner = new Person
                {
                    Name = OwnerName
                }
            },
            Author = new Person
            {
                Name = AuthorName
            }
        };
    }
}
