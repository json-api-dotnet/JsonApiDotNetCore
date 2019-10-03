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
using JsonApiDotNetCore.Internal;
using System.Net;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using System.Linq;
using JsonApiDotNetCore.QueryServices.Contracts;
using JsonApiDotNetCore.Serialization;

namespace UnitTests.Services
{
    public class EntityResourceServiceMore
    {
        [Fact]
        public async Task TestCanGetAll()
        {

        }

        /// <summary>
        /// we expect the service layer to give use a 404 if there is no entity returned
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsync_Throw404OnNoEntityFound()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var jsonApiOptions = new JsonApiOptions
            {
                IncludeTotalRecordCount = false
            } as IJsonApiOptions;
            var repositoryMock = new Mock<IEntityRepository<Article>>();
            var queryManagerMock = new Mock<ICurrentRequest>();
            var pageManagerMock = new Mock<IPageQueryService>();
            var rgMock = new Mock<IResourceGraph>();
            var service = new CustomArticleService(repositoryMock.Object, jsonApiOptions, null, queryManagerMock.Object, pageManagerMock.Object, rgMock.Object);

            // Act / Assert
            var toExecute = new Func<Task>(() =>
            {
                return service.GetAsync(4);
            });
            var exception = await Assert.ThrowsAsync<JsonApiException>(toExecute);
            Assert.Equal(404, exception.GetStatusCode());
        }

        /// <summary>
        /// we expect the service layer to give use a 404 if there is no entity returned
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsync_ShouldThrow404OnNoEntityFoundWithRelationships()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var jsonApiOptions = new JsonApiOptions
            {
                IncludeTotalRecordCount = false
            } as IJsonApiOptions;
            var repositoryMock = new Mock<IEntityRepository<Article>>();

            var updatedFieldsMock = new Mock<IUpdatedFields>();
            var pageManagerMock = new Mock<IPageQueryService>();
            //updatedFieldsMock.Setup(qm => qm.Relationships).Returns(new List<string>() { "cookies" });
            //updatedFieldsMock.SetupGet(rm => rm.QuerySet).Returns(new QuerySet
            //{
            //    IncludedRelationships = new List<string> { "cookies" }
            //});
            var rgMock = new Mock<IResourceGraph>();
            var service = new CustomArticleService(repositoryMock.Object, jsonApiOptions, updatedFieldsMock.Object, null, pageManagerMock.Object, rgMock.Object);

            // Act / Assert
            var toExecute = new Func<Task>(() =>
            {
                return service.GetAsync(4);
            });
            var exception = await Assert.ThrowsAsync<JsonApiException>(toExecute);
            Assert.Equal(404, exception.GetStatusCode());
        }


    }
}
