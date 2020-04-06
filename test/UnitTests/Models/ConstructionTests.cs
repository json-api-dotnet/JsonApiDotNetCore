using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using Xunit;

namespace UnitTests.Models
{
    public sealed class ConstructionTests
    {
        [Fact]
        public void When_model_has_no_parameterless_contructor_it_must_fail()
        {
            // Arrange
            var graph = new ResourceGraphBuilder().AddResource<ResourceWithParameters>().Build();

            var serializer = new RequestDeserializer(graph, new TargetedFields());

            var body = new
            {
                data = new
                {
                    id = "1",
                    type = "resourceWithParameters"
                }
            };
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(body);

            // Act
            Action action = () => serializer.Deserialize(content);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal("Failed to create an instance of 'UnitTests.Models.ConstructionTests+ResourceWithParameters' using its default constructor.", exception.Message);
        }

        public class ResourceWithParameters : Identifiable
        {
            [Attr] public string Title { get; }

            public ResourceWithParameters(string title)
            {
                Title = title;
            }
        }
    }
}
