using Xunit;
using JsonApiDotNetCore.Routing;

namespace JsonApiDotNetCoreTests.Routing.UnitTests
{
    public class RoutBuilderTests
    {
      [Theory]
      [InlineData("api/v1","People","/api/v1/people")]
      [InlineData("api/v1","TodoItems","/api/v1/todo-items")]
      [InlineData("api","todoItems","/api/todo-items")]
      [InlineData("api","MoreModelsHere","/api/more-models-here")]
      public void BuildRoute_Returns_CorrectRoute(string nameSpace, string collectionName, string expectOutput)
      {
        // arrange
        // act
        var result = RouteBuilder.BuildRoute(nameSpace, collectionName);

        // assert
        Assert.Equal(expectOutput, result);
      }
    }
}
