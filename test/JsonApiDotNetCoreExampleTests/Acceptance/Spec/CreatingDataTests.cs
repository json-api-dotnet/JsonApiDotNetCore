using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class CreatingDataTests
    {
        private TestFixture<TestStartup> _fixture;
        private IJsonApiContext _jsonApiContext;
        private Faker<TodoItem> _todoItemFaker;

        public CreatingDataTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _jsonApiContext = fixture.GetService<IJsonApiContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Can_Create_Guid_Identifiable_Entity()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var context = _fixture.GetService<AppDbContext>();

            var owner = new JsonApiDotNetCoreExample.Models.Person();
            context.People.Add(owner);
            await context.SaveChangesAsync();

            var route = "/api/v1/todo-collections";
            var request = new HttpRequestMessage(httpMethod, route);
            var content = new
            {
                data = new
                {
                    type = "todo-collections",
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = owner.Id.ToString()
                            }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Create_Entity_With_Client_Generate_Id()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    id = $"{clientDefinedId}",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal, 
                        createdDate = DateTime.Now
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Entity_With_Client_Defined_Id_If_Configured()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var builder = new WebHostBuilder()
                .UseStartup<ClientGeneratedIdsStartup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            const int clientDefinedId = 9999;
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    id = $"{clientDefinedId}",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal,
                        createdDate = DateTime.Now
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(clientDefinedId, deserializedBody.Id);
        }


        [Fact]
        public async Task Can_Create_Guid_Identifiable_Entity_With_Client_Defined_Id_If_Configured()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<ClientGeneratedIdsStartup>();
            var httpMethod = new HttpMethod("POST");
            var server = new TestServer(builder);
            var client = server.CreateClient();
            
            var context = _fixture.GetService<AppDbContext>();

            var owner = new JsonApiDotNetCoreExample.Models.Person();
            context.People.Add(owner);
            await context.SaveChangesAsync();

            var route = "/api/v1/todo-collections";
            var request = new HttpRequestMessage(httpMethod, route);
            var clientDefinedId = Guid.NewGuid();
            var content = new
            {
                data = new
                {
                    type = "todo-collections",
                    id = $"{clientDefinedId}",
                    relationships = new
                    {
                        owner = new
                        {
                            data = new
                            {
                                type = "people",
                                id = owner.Id.ToString()
                            }
                        }
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItemCollection)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(clientDefinedId, deserializedBody.Id);
        }

        [Fact]
        public async Task Can_Create_And_Set_HasMany_Relationships()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var context = _fixture.GetService<AppDbContext>();

            var owner = new JsonApiDotNetCoreExample.Models.Person();
            var todoItem = new TodoItem();
            todoItem.Owner = owner;
            context.People.Add(owner);
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var route = "/api/v1/todo-collections";
            var request = new HttpRequestMessage(httpMethod, route);
            var content = new
            {
                data = new
                {
                    type = "todo-collections",
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "owner",  new {
                            data = new
                            {
                                type = "people",
                                id = owner.Id.ToString()
                            }
                        } },
                        {  "todo-items", new {
                            data = new dynamic[]
                            {
                                new {
                                    type = "todo-items",
                                    id = todoItem.Id.ToString()
                                }
                            }
                        } }
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItemCollection)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);
            var newId = deserializedBody.Id;

            context = _fixture.GetService<AppDbContext>();
            var contextCollection = context.TodoItemCollections
                .Include(c => c.Owner)
                .Include(c => c.TodoItems)
                .SingleOrDefault(c => c.Id == newId);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(owner.Id, contextCollection.OwnerId);
            Assert.NotEmpty(contextCollection.TodoItems);
        }

        [Fact]
        public async Task ShouldReceiveLocationHeader_InResponse()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "todo-items",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal,
                        createdDate = DateTime.Now
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = (TodoItem)_fixture.GetService<IJsonApiDeSerializer>().Deserialize(body);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"/api/v1/todo-items/{deserializedBody.Id}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Respond_409_ToIncorrectEntityType()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todo-items";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var todoItem = _todoItemFaker.Generate();
            var content = new
            {
                data = new
                {
                    type = "people",
                    attributes = new
                    {
                        description = todoItem.Description,
                        ordinal = todoItem.Ordinal,
                        createdDate = DateTime.Now
                    }
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}
