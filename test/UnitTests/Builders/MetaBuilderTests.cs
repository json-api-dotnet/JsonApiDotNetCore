//using System.Collections.Generic;
//using JsonApiDotNetCore.Builders;
//using Xunit;

//namespace UnitTests.Builders
//{
//    public class MetaBuilderTests
//    {
//        [Fact]
//        public void Can_Add_Key_Value()
//        {
//            // arrange
//            var builder = new MetaBuilder();
//            var key = "test";
//            var value = "testValue";

//            // act
//            builder.Add(key, value);
//            var result = builder.Build();

//            // assert
//            Assert.NotEmpty(result);
//            Assert.Equal(value, result[key]);
//        }

//        [Fact]
//        public void Can_Add_Multiple_Values()
//        {
//            // arrange
//            var builder = new MetaBuilder();
//            var input = new Dictionary<string, object> {
//            { "key1", "value1" },
//            { "key2", "value2" }
//           };

//            // act
//            builder.Add(input);
//            var result = builder.Build();

//            // assert
//            Assert.NotEmpty(result);
//            foreach (var entry in input)
//                Assert.Equal(input[entry.Key], result[entry.Key]);
//        }

//        [Fact]
//        public void When_Adding_Duplicate_Values_Keep_Newest()
//        {
//            // arrange
//            var builder = new MetaBuilder();
            
//            var key = "key";
//            var oldValue = "oldValue";
//            var newValue = "newValue";
            
//            builder.Add(key, oldValue);

//            var input = new Dictionary<string, object> {
//                { key, newValue },
//                { "key2", "value2" }
//            };

//            // act
//            builder.Add(input);
//            var result = builder.Build();

//            // assert
//            Assert.NotEmpty(result);
//            Assert.Equal(input.Count, result.Count);
//            Assert.Equal(input[key], result[key]);
//        }
//    }
//}
