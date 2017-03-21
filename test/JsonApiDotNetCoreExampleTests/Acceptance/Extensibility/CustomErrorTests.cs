using Newtonsoft.Json;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    public class CustomErrorTests
    {
        [Fact]
        public void Can_Return_Custom_Error_Types()
        {
            // arrange
            var error = new CustomError("507", "title", "detail", "custom");
            var errorCollection = new ErrorCollection();
            errorCollection.Add(error);

            var expectedJson = JsonConvert.SerializeObject(new {
                errors = new dynamic[] {
                    new { 
                        myCustomProperty = "custom",
                        title = "title",
                        detail = "detail",
                        status = "507"                        
                    }
                }
            });

            // act
            var result = new JsonApiSerializer(null, null, null)
                .Serialize(errorCollection);

            // assert
            Assert.Equal(expectedJson, result);

        }

        class CustomError : Error {
            public CustomError(string status, string title, string detail, string myProp)
            : base(status, title, detail)
            {
                MyCustomProperty = myProp;
            }
            public string MyCustomProperty { get; set; }
        }
    }
}
