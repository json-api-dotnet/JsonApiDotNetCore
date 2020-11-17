using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Includes
{
    public sealed class IncludeTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public IncludeTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped<IResourceService<Article>, JsonApiResourceService<Article>>();
            });

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = null;
        }

        [Fact]
        public async Task Can_include_in_primary_resources()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                Author = new Author
                {
                    LastName = "Smith"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?include=author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.StringId);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(article.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("authors");
            responseDocument.Included[0].Id.Should().Be(article.Author.StringId);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(article.Author.LastName);
        }

        [Fact]
        public async Task Can_include_in_primary_resource_by_ID()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                Author = new Author
                {
                    LastName = "Smith"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("authors");
            responseDocument.Included[0].Id.Should().Be(article.Author.StringId);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(article.Author.LastName);
        }

        [Fact]
        public async Task Can_include_in_secondary_resource()
        {
            // Arrange
            var blog = new Blog
            {
                Owner = new Author
                {
                    LastName = "Smith",
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/owner?include=articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.Owner.StringId);
            responseDocument.SingleData.Attributes["lastName"].Should().Be(blog.Owner.LastName);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("articles");
            responseDocument.Included[0].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Owner.Articles[0].Caption);
        }

        [Fact]
        public async Task Can_include_in_secondary_resources()
        {
            // Arrange
            var blog = new Blog
            {
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "One",
                        Author = new Author
                        {
                            LastName = "Smith"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles?include=author";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[0].StringId);
            responseDocument.ManyData[0].Attributes["caption"].Should().Be(blog.Articles[0].Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("authors");
            responseDocument.Included[0].Id.Should().Be(blog.Articles[0].Author.StringId);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(blog.Articles[0].Author.LastName);
        }

        [Fact]
        public async Task Can_include_HasOne_relationships()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Description = "Work",
                Owner = new Person
                {
                    FirstName = "Joel"
                },
                Assignee = new Person
                {
                    FirstName = "James"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/todoItems/{todoItem.StringId}?include=owner,assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included.Should().HaveCount(2);
            
            responseDocument.Included[0].Type.Should().Be("people");
            responseDocument.Included[0].Id.Should().Be(todoItem.Owner.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(todoItem.Owner.FirstName);

            responseDocument.Included[1].Type.Should().Be("people");
            responseDocument.Included[1].Id.Should().Be(todoItem.Assignee.StringId);
            responseDocument.Included[1].Attributes["firstName"].Should().Be(todoItem.Assignee.FirstName);
        }

        [Fact]
        public async Task Can_include_HasMany_relationship()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                Revisions = new List<Revision>
                {
                    new Revision
                    {
                        PublishTime = 24.July(2019)
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=revisions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("revisions");
            responseDocument.Included[0].Id.Should().Be(article.Revisions.Single().StringId);
            responseDocument.Included[0].Attributes["publishTime"].Should().Be(article.Revisions.Single().PublishTime);
        }

        [Fact]
        public async Task Can_include_HasManyThrough_relationship()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                ArticleTags = new HashSet<ArticleTag>
                {
                    new ArticleTag
                    {
                        Tag = new Tag
                        {
                            Name = "Hot"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("tags");
            responseDocument.Included[0].Id.Should().Be(article.ArticleTags.Single().Tag.StringId);
            responseDocument.Included[0].Attributes["name"].Should().Be(article.ArticleTags.Single().Tag.Name);
        }

        [Fact]
        public async Task Can_include_HasManyThrough_relationship_in_secondary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                ArticleTags = new HashSet<ArticleTag>
                {
                    new ArticleTag
                    {
                        Tag = new Tag
                        {
                            Name = "Hot"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}/tags?include=articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Id.Should().Be(article.ArticleTags.ElementAt(0).Tag.StringId);
            responseDocument.ManyData[0].Attributes["name"].Should().Be(article.ArticleTags.Single().Tag.Name);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("articles");
            responseDocument.Included[0].Id.Should().Be(article.StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(article.Caption);
        }

        [Fact]
        public async Task Can_include_chain_of_HasOne_relationships()
        {
            // Arrange
            var article = new Article
            {
                Caption = "One",
                Author = new Author
                {
                    LastName = "Smith",
                    LivingAddress = new Address
                    {
                        Street = "Main Road",
                        Country = new Country
                        {
                            Name = "United States of America"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=author.livingAddress.country";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);
            responseDocument.SingleData.Attributes["caption"].Should().Be(article.Caption);

            responseDocument.Included.Should().HaveCount(3);
            
            responseDocument.Included[0].Type.Should().Be("authors");
            responseDocument.Included[0].Id.Should().Be(article.Author.StringId);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(article.Author.LastName);
            
            responseDocument.Included[1].Type.Should().Be("addresses");
            responseDocument.Included[1].Id.Should().Be(article.Author.LivingAddress.StringId);
            responseDocument.Included[1].Attributes["street"].Should().Be(article.Author.LivingAddress.Street);
            
            responseDocument.Included[2].Type.Should().Be("countries");
            responseDocument.Included[2].Id.Should().Be(article.Author.LivingAddress.Country.StringId);
            responseDocument.Included[2].Attributes["name"].Should().Be(article.Author.LivingAddress.Country.Name);
        }

        [Fact]
        public async Task Can_include_chain_of_HasMany_relationships()
        {
            // Arrange
            var blog = new Blog
            {
                Title = "Some",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "One",
                        Revisions = new List<Revision>
                        {
                            new Revision
                            {
                                PublishTime = 24.July(2019)
                            }
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}?include=articles.revisions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.StringId);
            responseDocument.SingleData.Attributes["title"].Should().Be(blog.Title);

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Type.Should().Be("articles");
            responseDocument.Included[0].Id.Should().Be(blog.Articles[0].StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Articles[0].Caption);
            
            responseDocument.Included[1].Type.Should().Be("revisions");
            responseDocument.Included[1].Id.Should().Be(blog.Articles[0].Revisions.Single().StringId);
            responseDocument.Included[1].Attributes["publishTime"].Should().Be(blog.Articles[0].Revisions.Single().PublishTime);
        }

        [Fact]
        public async Task Can_include_chain_of_recursive_relationships()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Description = "Root",
                Collection = new TodoItemCollection
                {
                    Name = "Primary",
                    Owner = new Person
                    {
                        FirstName = "Jack"
                    },
                    TodoItems = new HashSet<TodoItem>
                    {
                        new TodoItem
                        {
                            Description = "This is nested.",
                            Owner = new Person
                            {
                                FirstName = "Jill"
                            }
                        },
                        new TodoItem
                        {
                            Description = "This is nested too."
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });
            
            string route = $"/api/v1/todoItems/{todoItem.StringId}?include=collection.todoItems.owner";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included.Should().HaveCount(5);

            responseDocument.Included[0].Type.Should().Be("todoCollections");
            responseDocument.Included[0].Id.Should().Be(todoItem.Collection.StringId);
            responseDocument.Included[0].Attributes["name"].Should().Be(todoItem.Collection.Name);
            
            responseDocument.Included[1].Type.Should().Be("todoItems");
            responseDocument.Included[1].Id.Should().Be(todoItem.StringId);
            responseDocument.Included[1].Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included[2].Type.Should().Be("todoItems");
            responseDocument.Included[2].Id.Should().Be(todoItem.Collection.TodoItems.First().StringId);
            responseDocument.Included[2].Attributes["description"].Should().Be(todoItem.Collection.TodoItems.First().Description);

            responseDocument.Included[3].Type.Should().Be("people");
            responseDocument.Included[3].Id.Should().Be(todoItem.Collection.TodoItems.First().Owner.StringId);
            responseDocument.Included[3].Attributes["firstName"].Should().Be(todoItem.Collection.TodoItems.First().Owner.FirstName);

            responseDocument.Included[4].Type.Should().Be("todoItems");
            responseDocument.Included[4].Id.Should().Be(todoItem.Collection.TodoItems.Skip(1).First().StringId);
            responseDocument.Included[4].Attributes["description"].Should().Be(todoItem.Collection.TodoItems.Skip(1).First().Description);
        }

        [Fact]
        public async Task Can_include_chain_of_relationships_with_multiple_paths()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Description = "Root",
                Collection = new TodoItemCollection
                {
                    Name = "Primary",
                    Owner = new Person
                    {
                        FirstName = "Jack",
                        Role = new PersonRole()
                    },
                    TodoItems = new HashSet<TodoItem>
                    {
                        new TodoItem
                        {
                            Description = "This is nested.",
                            Owner = new Person
                            {
                                FirstName = "Jill"
                            }
                        },
                        new TodoItem
                        {
                            Description = "This is nested too."
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });
            
            string route = $"/api/v1/todoItems/{todoItem.StringId}?include=collection.owner.role,collection.todoItems.owner";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included.Should().HaveCount(7);

            responseDocument.Included[0].Type.Should().Be("todoCollections");
            responseDocument.Included[0].Id.Should().Be(todoItem.Collection.StringId);
            responseDocument.Included[0].Attributes["name"].Should().Be(todoItem.Collection.Name);

            responseDocument.Included[1].Type.Should().Be("people");
            responseDocument.Included[1].Id.Should().Be(todoItem.Collection.Owner.StringId);
            responseDocument.Included[1].Attributes["firstName"].Should().Be(todoItem.Collection.Owner.FirstName);

            responseDocument.Included[2].Type.Should().Be("personRoles");
            responseDocument.Included[2].Id.Should().Be(todoItem.Collection.Owner.Role.StringId);

            responseDocument.Included[3].Type.Should().Be("todoItems");
            responseDocument.Included[3].Id.Should().Be(todoItem.StringId);
            responseDocument.Included[3].Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included[4].Type.Should().Be("todoItems");
            responseDocument.Included[4].Id.Should().Be(todoItem.Collection.TodoItems.First().StringId);
            responseDocument.Included[4].Attributes["description"].Should().Be(todoItem.Collection.TodoItems.First().Description);

            responseDocument.Included[5].Type.Should().Be("people");
            responseDocument.Included[5].Id.Should().Be(todoItem.Collection.TodoItems.First().Owner.StringId);
            responseDocument.Included[5].Attributes["firstName"].Should().Be(todoItem.Collection.TodoItems.First().Owner.FirstName);

            responseDocument.Included[6].Type.Should().Be("todoItems");
            responseDocument.Included[6].Id.Should().Be(todoItem.Collection.TodoItems.Skip(1).First().StringId);
            responseDocument.Included[6].Attributes["description"].Should().Be(todoItem.Collection.TodoItems.Skip(1).First().Description);
        }

        [Fact]
        public async Task Prevents_duplicate_includes_over_single_resource()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "Janice"
            };

            var todoItem = new TodoItem
            {
                Description = "Root",
                Owner = person,
                Assignee = person
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/todoItems/{todoItem.StringId}?include=owner&include=assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(todoItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(todoItem.Description);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("people");
            responseDocument.Included[0].Id.Should().Be(person.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(person.FirstName);
        }

        [Fact]
        public async Task Prevents_duplicate_includes_over_multiple_resources()
        {
            // Arrange
            var person = new Person
            {
                FirstName = "Janice"
            };

            var todoItems = new List<TodoItem>
            {
                new TodoItem
                {
                    Description = "First",
                    Owner = person
                },
                new TodoItem
                {
                    Description = "Second",
                    Owner = person
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.TodoItems.AddRange(todoItems);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/todoItems?include=owner";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("people");
            responseDocument.Included[0].Id.Should().Be(person.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(person.FirstName);
        }

        [Fact]
        public async Task Cannot_include_unknown_relationship()
        {
            // Arrange
            var route = "/api/v1/people?include=doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified include is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'people'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Cannot_include_unknown_nested_relationship()
        {
            // Arrange
            var route = "/api/v1/people?include=todoItems.doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified include is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'todoItems.doesNotExist' does not exist on resource 'todoItems'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Cannot_include_relationship_with_blocked_capability()
        {
            // Arrange
            var route = "/api/v1/people?include=unIncludeableItem";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Including the requested relationship is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Including the relationship 'unIncludeableItem' on 'people' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Ignores_null_parent_in_nested_include()
        {
            // Arrange
            var todoItems = new List<TodoItem>
            {
                new TodoItem
                {
                    Description = "Owned",
                    Owner = new Person
                    {
                        FirstName = "Julian"
                    }
                },
                new TodoItem
                {
                    Description = "Unowned"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.TodoItems.AddRange(todoItems);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/todoItems?include=owner.role";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.ManyData.Should().HaveCount(2);

            var resourcesWithOwner = responseDocument.ManyData.Where(resource => resource.Relationships.First(pair => pair.Key == "owner").Value.SingleData != null).ToArray();
            resourcesWithOwner.Should().HaveCount(1);
            resourcesWithOwner[0].Attributes["description"].Should().Be(todoItems[0].Description);

            var resourcesWithoutOwner = responseDocument.ManyData.Where(resource => resource.Relationships.First(pair => pair.Key == "owner").Value.SingleData == null).ToArray();
            resourcesWithoutOwner.Should().HaveCount(1);
            resourcesWithoutOwner[0].Attributes["description"].Should().Be(todoItems[1].Description);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("people");
            responseDocument.Included[0].Id.Should().Be(todoItems[0].Owner.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(todoItems[0].Owner.FirstName);
        }

        [Fact]
        public async Task Can_include_at_configured_maximum_inclusion_depth()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = 1;

            var blog = new Blog();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles?include=author,revisions";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_exceed_configured_maximum_inclusion_depth()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = 1;

            var route = "/api/v1/blogs/123/owner?include=articles.revisions";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified include is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Including 'articles.revisions' exceeds the maximum inclusion depth of 1.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("include");
        }
    }
}
