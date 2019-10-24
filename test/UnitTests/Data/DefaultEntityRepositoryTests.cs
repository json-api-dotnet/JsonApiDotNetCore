using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using System.Threading.Tasks;
using System.Linq;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Managers.Contracts;

namespace UnitTests.Data
{
    public class DefaultResourceRepositoryTests : JsonApiControllerMixin
    {
        private readonly Mock<DbSet<TodoItem>> _dbSetMock;
        private readonly Mock<DbContext> _contextMock;
        private readonly Mock<ITargetedFields> _targetedFieldsMock;
        private readonly Mock<IDbContextResolver> _contextResolverMock;
        private readonly TodoItem _todoItem;

        public DefaultResourceRepositoryTests()
        {
            _todoItem = new TodoItem
            {
                Id = 1,
                Description = Guid.NewGuid().ToString(),
                Ordinal = 10
            };
            _dbSetMock = DbSetMock.Create(new[] { _todoItem });
            _contextMock = new Mock<DbContext>();
            _contextResolverMock = new Mock<IDbContextResolver>();
            _targetedFieldsMock = new Mock<ITargetedFields>();
        }



        private DefaultResourceRepository<TodoItem> GetRepository()
        {

            _contextMock
                .Setup(m => m.Set<TodoItem>())
                .Returns(_dbSetMock.Object);

            _contextResolverMock
                .Setup(m => m.GetContext())
                .Returns(_contextMock.Object);

            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItem>().Build();


            return new DefaultResourceRepository<TodoItem>(
                _targetedFieldsMock.Object,
                _contextResolverMock.Object,
                resourceGraph, null, null);
        }






        private static TodoItem[] TodoItems(params int[] ids)
        {
            return ids.Select(id => new TodoItem { Id = id }).ToArray();
        }

        private class IdComparer<T> : IEqualityComparer<T>
            where T : IIdentifiable
        {
            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

            public int GetHashCode(T obj) => obj?.StringId?.GetHashCode() ?? 0;
        }
    }
}
