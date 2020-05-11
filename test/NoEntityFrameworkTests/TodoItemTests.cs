using JsonApiDotNetCore;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace NoEntityFrameworkTests
{
    public sealed class TodoItemTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public TodoItemTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_Get_TodoItems()
        {
            // Arrange
            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(new TodoItem());
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/todoItems");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.NotEmpty(document.ManyData);
        }

        [Fact]
        public async Task Can_Get_TodoItem_By_Id()
        {
            // Arrange
            var todoItem = new TodoItem();

            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/todoItems/" + todoItem.StringId);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.NotNull(document.SingleData);
            Assert.Equal(todoItem.StringId, document.SingleData.Id);
        }

        [Fact]
        public async Task Can_Create_TodoItem()
        {
            // Arrange
            var description = Guid.NewGuid().ToString();

            var requestContent = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description,
                        ordinal = 1
                    }
                }
            };

            var requestBody = JsonConvert.SerializeObject(requestContent);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/todoItems/")
            {
                Content = new StringContent(requestBody)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var client = _factory.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.Created, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.NotNull(document.SingleData);
            Assert.Equal(description, document.SingleData.Attributes["description"]);
        }

        [Fact]
        public async Task Can_Delete_TodoItem()
        {
            // Arrange
            var todoItem = new TodoItem();

            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/todoItems/" + todoItem.StringId);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.NoContent, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.Null(document.Data);
        }

        private async Task ExecuteOnDbContextAsync(Func<AppDbContext, Task> asyncAction)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await asyncAction(dbContext);
        }

        private static void AssertStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            if (expected != response.StatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                Assert.True(false, $"Got {response.StatusCode} status code instead of {expected}. Payload: {responseBody}");
            }
        }
    }
}
