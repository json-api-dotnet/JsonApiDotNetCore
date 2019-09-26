//using System;
//using System.Collections.Generic;
//using JsonApiDotNetCore.Controllers;
//using Xunit;
//using Moq;
//using Microsoft.EntityFrameworkCore;
//using JsonApiDotNetCoreExample.Models;
//using JsonApiDotNetCore.Extensions;
//using JsonApiDotNetCore.Data;
//using JsonApiDotNetCore.Models;
//using Microsoft.Extensions.Logging;
//using JsonApiDotNetCore.Services;
//using System.Threading.Tasks;
//using System.Linq;

//namespace UnitTests.Data
//{
//    public class DefaultEntityRepository_Tests : JsonApiControllerMixin
//    {
//        private readonly Mock<IJsonApiContext> _jsonApiContextMock;
//        private readonly Mock<ILoggerFactory> _loggFactoryMock;
//        private readonly Mock<DbSet<TodoItem>> _dbSetMock;
//        private readonly Mock<DbContext> _contextMock;
//        private readonly Mock<IDbContextResolver> _contextResolverMock;
//        private readonly TodoItem _todoItem;
//        private Dictionary<AttrAttribute, object> _attrsToUpdate = new Dictionary<AttrAttribute, object>();
//        private Dictionary<RelationshipAttribute, object> _relationshipsToUpdate = new Dictionary<RelationshipAttribute, object>();

//        public DefaultEntityRepository_Tests()
//        {
//            _todoItem = new TodoItem
//            {
//                Id = 1,
//                Description = Guid.NewGuid().ToString(),
//                Ordinal = 10
//            };
//            _jsonApiContextMock = new Mock<IJsonApiContext>();
//            _loggFactoryMock = new Mock<ILoggerFactory>();
//            _dbSetMock = DbSetMock.Create<TodoItem>(new[] { _todoItem });
//            _contextMock = new Mock<DbContext>();
//            _contextResolverMock = new Mock<IDbContextResolver>();
//        }

//        [Fact]
//        public async Task UpdateAsync_Updates_Attributes_In_AttributesToUpdate()
//        {
//            // arrange
//            var todoItemUpdates = new TodoItem
//            {
//                Id = _todoItem.Id,
//                Description = Guid.NewGuid().ToString()
//            };

//            var descAttr = new AttrAttribute("description", "Description");
//            descAttr.PropertyInfo = typeof(TodoItem).GetProperty(nameof(TodoItem.Description));

//            _attrsToUpdate = new Dictionary<AttrAttribute, object> 
//            {
//                {
//                    descAttr,
//                    null //todoItemUpdates.Description
//                }
//            };

//            var repository = GetRepository();

//            // act
//            var updatedItem = await repository.UpdateAsync(todoItemUpdates);

//            // assert
//            Assert.NotNull(updatedItem);
//            Assert.Equal(_todoItem.Ordinal, updatedItem.Ordinal);
//            Assert.Equal(todoItemUpdates.Description, updatedItem.Description);
//        }

//        private DefaultEntityRepository<TodoItem> GetRepository()
//        {

//            _contextMock
//                .Setup(m => m.Set<TodoItem>())
//                .Returns(_dbSetMock.Object);

//            _contextResolverMock
//                .Setup(m => m.GetContext())
//                .Returns(_contextMock.Object);

//            _jsonApiContextMock
//               .Setup(m => m.RequestManager.GetUpdatedAttributes())
//               .Returns(_attrsToUpdate);

//            _jsonApiContextMock
//                .Setup(m => m.RequestManager.GetUpdatedRelationships())
//                .Returns(_relationshipsToUpdate);


//            return new DefaultEntityRepository<TodoItem>(
//                _loggFactoryMock.Object,
//                _jsonApiContextMock.Object,
//                _contextResolverMock.Object);
//        }

//        [Theory]
//        [InlineData(0)]
//        [InlineData(-1)]
//        [InlineData(-10)]
//        public async Task Page_When_PageSize_Is_NonPositive_Does_Nothing(int pageSize)
//        {
//            var todoItems = DbSetMock.Create(TodoItems(2, 3, 1)).Object;
//            var repository = GetRepository();

//            var result = await repository.PageAsync(todoItems, pageSize, 3);

//            Assert.Equal(TodoItems(2, 3, 1), result, new IdComparer<TodoItem>());
//        }

//        [Fact]
//        public async Task Page_When_PageNumber_Is_Zero_Pretends_PageNumber_Is_One()
//        {
//            var todoItems = DbSetMock.Create(TodoItems(2, 3, 1)).Object;
//            var repository = GetRepository();

//            var result = await repository.PageAsync(todoItems, 1, 0);

//            Assert.Equal(TodoItems(2), result, new IdComparer<TodoItem>());
//        }

//        [Fact]
//        public async Task Page_When_PageNumber_Of_PageSize_Does_Not_Exist_Return_Empty_Queryable()
//        {
//            var todoItems = DbSetMock.Create(TodoItems(2, 3, 1)).Object;
//            var repository = GetRepository();

//            var result = await repository.PageAsync(todoItems, 2, 3);

//            Assert.Empty(result);
//        }

//        [Theory]
//        [InlineData(3, 2, new[] { 4, 5, 6 })]
//        [InlineData(8, 2, new[] { 9 })]
//        [InlineData(20, 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
//        public async Task Page_When_PageNumber_Is_Positive_Returns_PageNumberTh_Page_Of_Size_PageSize(int pageSize, int pageNumber, int[] expectedResult)
//        {
//            var todoItems = DbSetMock.Create(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9)).Object;
//            var repository = GetRepository();

//            var result = await repository.PageAsync(todoItems, pageSize, pageNumber);

//            Assert.Equal(TodoItems(expectedResult), result, new IdComparer<TodoItem>());
//        }

//        [Theory]
//        [InlineData(6, -1, new[] { 4, 5, 6, 7, 8, 9 })]
//        [InlineData(6, -2, new[] { 1, 2, 3 })]
//        [InlineData(20, -1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
//        public async Task Page_When_PageNumber_Is_Negative_Returns_PageNumberTh_Page_From_End(int pageSize, int pageNumber, int[] expectedIds)
//        {
//            var todoItems = DbSetMock.Create(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9)).Object;
//            var repository = GetRepository();

//            var result = await repository.PageAsync(todoItems, pageSize, pageNumber);
            
//            Assert.Equal(TodoItems(expectedIds), result, new IdComparer<TodoItem>());
//        }

//        private static TodoItem[] TodoItems(params int[] ids)
//        {
//            return ids.Select(id => new TodoItem { Id = id }).ToArray();
//        }

//        private class IdComparer<T> : IEqualityComparer<T>
//            where T : IIdentifiable
//        {
//            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

//            public int GetHashCode(T obj) => obj?.StringId?.GetHashCode() ?? 0;
//        }
//    }
//}
