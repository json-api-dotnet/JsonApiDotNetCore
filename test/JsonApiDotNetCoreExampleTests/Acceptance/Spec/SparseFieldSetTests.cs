using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Xunit;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCoreExample.Models;
using System.Linq;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class SparseFieldSetTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private readonly AppDbContext _dbContext;

        public SparseFieldSetTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Can_Select_Sparse_Fieldsets()
        {
            // arrange
            var fields = new string[] { "Id", "Description" };
            var todoItem = new TodoItem {
                Description = "description",
                Ordinal = 1
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();

            // act
            var result = await _dbContext
                .TodoItems
                .Where(t=>t.Id == todoItem.Id)
                .Select(fields)
                .FirstAsync();

            // assert
            Assert.Equal(0, result.Ordinal);
            Assert.Equal(todoItem.Description, result.Description);
        }
    }
}
