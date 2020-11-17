using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using Xunit;

namespace NoEntityFrameworkTests
{
    public sealed class WorkItemTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public WorkItemTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_Get_WorkItems()
        {
            // Arrange
            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(new WorkItem());
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/workItems");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.NotEmpty(document.ManyData);
        }

        [Fact]
        public async Task Can_Get_WorkItem_By_Id()
        {
            // Arrange
            var workItem = new WorkItem();

            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/workItems/" + workItem.StringId);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.OK, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<Document>(responseBody);

            Assert.NotNull(document.SingleData);
            Assert.Equal(workItem.StringId, document.SingleData.Id);
        }

        [Fact]
        public async Task Can_Create_WorkItem()
        {
            // Arrange
            var title = Guid.NewGuid().ToString();

            var requestContent = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        title,
                        ordinal = 1
                    }
                }
            };

            var requestBody = JsonConvert.SerializeObject(requestContent);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/workItems/")
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
            Assert.Equal(title, document.SingleData.Attributes["title"]);
        }

        [Fact]
        public async Task Can_Delete_WorkItem()
        {
            // Arrange
            var workItem = new WorkItem();

            await ExecuteOnDbContextAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/workItems/" + workItem.StringId);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            AssertStatusCode(HttpStatusCode.NoContent, response);

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseBody);
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
                Assert.True(expected == response.StatusCode, $"Got {response.StatusCode} status code instead of {expected}. Response body: {responseBody}");
            }
        }
    }
}
