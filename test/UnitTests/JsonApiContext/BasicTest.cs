using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Controllers;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCoreExample.Services;
using JsonApiDotNetCore.Data;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;

namespace UnitTests.JsonApiContext
{
    public class BasicTest
    {
        [Fact]
        public async Task CanTestController()
        {
            // Arrange
            var jsonApiContext = new Mock<IJsonApiContext>();
            var serviceMock = new Mock<IResourceService<Article>>();
            var controller = new ArticlesController(jsonApiContext.Object,serviceMock.Object);

            // Act 
            var result = await controller.GetAsync();

            // Assert
            var okResult =  Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as IEnumerable<Article>;

            Assert.NotNull(value);
        }

        [Fact]
        public async Task CanTestService()
        {
            // Arrange
            var jacMock = FetchContextMock();
            var loggerMock = new Mock<ILoggerFactory>();
            var jsonApiOptionsMock = new Mock<IJsonApiOptions>();
            var repositoryMock = new Mock<IEntityRepository<Article>>();

            var service = new CustomArticleService(jacMock.Object, repositoryMock.Object, jsonApiOptionsMock.Object, loggerMock.Object);
            // Act
            var result = await service.GetAsync();

            // Assert
            Assert.NotNull(result);
        }

        public Mock<IJsonApiContext> FetchContextMock()
        {
            return new Mock<IJsonApiContext>();
        }

    }
}
