using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Services;
using System.Threading.Tasks;

namespace UnitTests.Data
{
    public class DefaultEntityRepository_Tests : JsonApiControllerMixin
    {
        private readonly Mock<IJsonApiContext> _jsonApiContextMock;
        private readonly Mock<ILoggerFactory> _loggFactoryMock;
        private readonly Mock<DbSet<TodoItem>> _dbSetMock;
        private readonly Mock<DbContext> _contextMock;
        private readonly Mock<IDbContextResolver> _contextResolverMock;
        private readonly TodoItem _todoItem;
        private Dictionary<AttrAttribute, object> _attrsToUpdate = new Dictionary<AttrAttribute, object>();
        private Dictionary<RelationshipAttribute, object> _relationshipsToUpdate = new Dictionary<RelationshipAttribute, object>();

        public DefaultEntityRepository_Tests()
        {
            _todoItem = new TodoItem
            {
                Id = 1,
                Description = Guid.NewGuid().ToString(),
                Ordinal = 10
            };
            _jsonApiContextMock = new Mock<IJsonApiContext>();
            _loggFactoryMock = new Mock<ILoggerFactory>();
            _dbSetMock = DbSetMock.Create<TodoItem>(new[] { _todoItem });
            _contextMock = new Mock<DbContext>();
            _contextResolverMock = new Mock<IDbContextResolver>();
        }

        [Fact]
        public async Task UpdateAsync_Updates_Attributes_In_AttributesToUpdate()
        {
            // arrange
            var todoItemUpdates = new TodoItem
            {
                Id = _todoItem.Id,
                Description = Guid.NewGuid().ToString()
            };

            _attrsToUpdate = new Dictionary<AttrAttribute, object> 
            {
                {
                    new AttrAttribute("description", "Description"),
                    todoItemUpdates.Description
                }
            };

            var repository = GetRepository();

            // act
            var updatedItem = await repository.UpdateAsync(_todoItem.Id, todoItemUpdates);

            // assert
            Assert.NotNull(updatedItem);
            Assert.Equal(_todoItem.Ordinal, updatedItem.Ordinal);
            Assert.Equal(todoItemUpdates.Description, updatedItem.Description);
        }

        private DefaultEntityRepository<TodoItem> GetRepository()
        {
            _contextResolverMock
                .Setup(m => m.GetContext())
                .Returns(_contextMock.Object);

            _contextResolverMock
                .Setup(m => m.GetDbSet<TodoItem>())
                .Returns(_dbSetMock.Object);

            _jsonApiContextMock
               .Setup(m => m.AttributesToUpdate)
               .Returns(_attrsToUpdate);

            _jsonApiContextMock
                .Setup(m => m.RelationshipsToUpdate)
                .Returns(_relationshipsToUpdate);

            return new DefaultEntityRepository<TodoItem>(
                _loggFactoryMock.Object,
                _jsonApiContextMock.Object,
                _contextResolverMock.Object);
        }
    }
}
