using System.Collections.Generic;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Models.Pointers;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Models.Pointers
{
    public class OperationsPointerTests
    {
        [Fact]
        public void GetValue_Can_Get_Value_From_Data_Id()
        {
            // arrange
            var json = @"[
                {
                    ""op"": ""add"",
                    ""data"": {
                        ""id"": ""1"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""dgeb""
                        }
                    }
                }]";
            var operations = JsonConvert.DeserializeObject<List<Operation>>(json);
            var pointerJson = @"{ ""pointer"": ""/operations/0/data/id"" }";
            var pointer = JsonConvert.DeserializeObject<OperationsPointer>(pointerJson);
            var value = pointer.GetValue(operations);
            Assert.Equal("1", value.ToString());
        }

        [Fact]
        public void GetValue_Can_Get_Value_From_Data_Type()
        {
            // arrange
            var json = @"[
                {
                    ""op"": ""add"",
                    ""data"": {
                        ""id"": ""1"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""dgeb""
                        }
                    }
                }]";
            var operations = JsonConvert.DeserializeObject<List<Operation>>(json);
            var pointerJson = @"{ ""pointer"": ""/operations/0/data/type"" }";
            var pointer = JsonConvert.DeserializeObject<OperationsPointer>(pointerJson);
            var value = pointer.GetValue(operations);
            Assert.Equal("authors", value.ToString());
        }

        [Fact]
        public void GetValue_Can_Get_Value_From_ListData_Id()
        {
            // arrange
            var json = @"[
                {
                    ""op"": ""get"",
                    ""data"": [{
                        ""id"": ""1"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""dgeb""
                        }
                    }, {
                        ""id"": ""2"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""jaredcnance""
                        }
                    }]
                }]";
            var operations = JsonConvert.DeserializeObject<List<Operation>>(json);
            var pointerJson = @"{ ""pointer"": ""/operations/0/data/1/id"" }";
            var pointer = JsonConvert.DeserializeObject<OperationsPointer>(pointerJson);
            var value = pointer.GetValue(operations);
            Assert.Equal("2", value.ToString());
        }

        [Fact]
        public void GetValue_Can_Get_Value_From_Second_Operations_Data_Id()
        {
            // arrange
            var json = @"[
                {
                    ""op"": ""get"",
                    ""data"": {
                        ""id"": ""1"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""dgeb""
                        }
                    }
                },{
                    ""op"": ""get"",
                    ""data"": {
                        ""id"": ""2"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""jaredcnance""
                        }
                    }
                }]";
            var operations = JsonConvert.DeserializeObject<List<Operation>>(json);
            var pointerJson = @"{ ""pointer"": ""/operations/1/data/id"" }";
            var pointer = JsonConvert.DeserializeObject<OperationsPointer>(pointerJson);
            var value = pointer.GetValue(operations);
            Assert.Equal("2", value.ToString());
        }

        [Fact]
        public void GetValue_Can_Get_Value_From_Second_Operations_Data_Type()
        {
            // arrange
            var json = @"[
                {
                    ""op"": ""get"",
                    ""data"": {
                        ""id"": ""1"",
                        ""type"": ""authors"",
                        ""attributes"": {
                            ""name"": ""dgeb""
                        }
                    }
                },{
                    ""op"": ""get"",
                    ""data"": {
                        ""id"": ""1"",
                        ""type"": ""articles"",
                        ""attributes"": {
                            ""name"": ""JSON API paints my bikeshed!""
                        }
                    }
                }]";
            var operations = JsonConvert.DeserializeObject<List<Operation>>(json);
            var pointerJson = @"{ ""pointer"": ""/operations/1/data/type"" }";
            var pointer = JsonConvert.DeserializeObject<OperationsPointer>(pointerJson);
            var value = pointer.GetValue(operations);
            Assert.Equal("articles", value.ToString());
        }
    }
}