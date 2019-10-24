using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using JsonApiDotNetCoreExample.Controllers;
using System.Reflection;
using Xunit;
using System.Linq;

namespace UnitTests.Routing
{
    public class DefaultRoutingConventionTests
    {

        [Fact]
        public void DefaultRoutingCustomRoute_CustomRouteRegistered_ShouldBeRegisteredAsTemplate()
        {
            // Arrange
            var mockOptions = new Mock<IJsonApiOptions>();
            var mockFormatter = new Mock<IResourceNameFormatter>();
            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItem>("customRouteForMe").Build();
            var convention = new DefaultRoutingConvention(mockOptions.Object, mockFormatter.Object, resourceGraph);
            var attributes = new List<object>().AsReadOnly();
            var controllerModel = new ControllerModel(typeof(TodoItemsController).GetTypeInfo(), attributes);
            controllerModel.ControllerName = "Test";
            var sModel = new SelectorModel();
            controllerModel.Selectors.Add(sModel);
            var appModel = new ApplicationModel();
            appModel.Controllers.Add(controllerModel);

            // Act 
            convention.Apply(appModel);

            // Assert
            Assert.Equal("/customRouteForMe", controllerModel.Selectors[0].AttributeRouteModel.Template);
        }
    }
}
