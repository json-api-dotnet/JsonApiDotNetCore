using Xunit;
using JsonApiDotNetCore.Routing.Query;
using Microsoft.AspNetCore.Http.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace JsonApiDotNetCoreTests.Routing.UnitTests.Query
{
    public class QuerySetTests
    {
        [Fact]
        public void QuerySetConstructor_BuildsObject_FromQueryCollection()
        {
          // arrange
          var queries = new Dictionary<string, StringValues>();
          queries.Add("filter[id]", "1");
          queries.Add("sort", new StringValues(new string[] { "-id", "name" }));
          var queryCollection = new QueryCollection(queries);

          // act
          var querySet = new QuerySet(queryCollection);

          // assert
          Assert.NotNull(querySet.Filter);
          Assert.Equal("Id", querySet.Filter.PropertyName);
          Assert.Equal("1", querySet.Filter.PropertyValue);

          Assert.NotNull(querySet.SortParameters);
          Assert.NotNull(querySet.SortParameters.SingleOrDefault(x=> x.Direction == SortDirection.Descending && x.PropertyName == "Id"));
          Assert.NotNull(querySet.SortParameters.SingleOrDefault(x=> x.Direction == SortDirection.Ascending && x.PropertyName == "Name"));
        }
    }
}
