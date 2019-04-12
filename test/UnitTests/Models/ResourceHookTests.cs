
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;


namespace UnitTests.Models
{
    public class ResourceHooks_Tests
    {


        public ResourceHooks_Tests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder().Build();
        }

        [Fact]
        public void Hook_Discovery()
        {
            // arrange & act
            var hookConfig = new ImplementedResourceHooks<Dummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
            Assert.Equal(2, hookConfig.ImplementedHooks.Length);
        }

        [Fact]
        public void BeforeCreate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.BeforeCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void AfterCreate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeRead_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.BeforeRead(It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }
        [Fact]
        public void AfterRead_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.AfterRead(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeUpdate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.BeforeUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterUpdate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.AfterUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeDelete_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();

            // act
            hookExecutor.BeforeDelete(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterDelete_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects();
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<Dummy>>(), true, It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<IEnumerable<Dummy>>(), true, It.IsAny<ResourceAction>()), Times.Once());
        }
        Mock<DummyResourceDefinition> CreateResourceDefinitionMock()
        {
            var resourceDefinition = new Mock<DummyResourceDefinition>();
            resourceDefinition.Setup(rd => rd.BeforeCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterCreate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), null));
            resourceDefinition.Setup(rd => rd.AfterRead(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeDelete(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterDelete(It.IsAny<IEnumerable<Dummy>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()));
            return resourceDefinition;
        }

        (Mock<DummyResourceDefinition>, Mock<IJsonApiContext>, IResourceHookExecutor<Dummy>) CreateTestObjects()
        {

            var resourceDefinition = CreateResourceDefinitionMock();

            var processorFactory = new Mock<IGenericProcessorFactory>();
            processorFactory.Setup(c => c.GetProcessor<IResourceDefinition>(It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns((IResourceDefinition)resourceDefinition.Object);
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);

            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<Dummy>(context.Object, new ImplementedResourceHooks<Dummy>(), meta);

            return (resourceDefinition, context, hookExecutor);
        }

    }

    public class Dummy : Identifiable
    {

    }
    public class DummyResourceDefinition : ResourceDefinition<Dummy>
    {
        public override void BeforeDelete(IEnumerable<Dummy> entities, ResourceAction actionSource)
        {
        }
        public override void AfterDelete(IEnumerable<Dummy> entities, bool succeeded, ResourceAction actionSource)
        {

        }
    }


}
