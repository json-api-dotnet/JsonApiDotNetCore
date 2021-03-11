using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class DiscoveryTests
    {
        [Fact]
        public void HookDiscovery_StandardResourceDefinition_CanDiscover()
        {
            // Act
            var hookConfig = new HooksDiscovery<Dummy>(MockProvider<Dummy>(new DummyResourceDefinition()));

            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        [Fact]
        public void HookDiscovery_InheritanceSubclass_CanDiscover()
        {
            // Act
            var hookConfig = new HooksDiscovery<AnotherDummy>(MockProvider<AnotherDummy>(new AnotherDummyResourceDefinition()));

            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        [Fact]
        public void HookDiscovery_WronglyUsedLoadDatabaseValueAttribute_ThrowsJsonApiSetupException()
        {
            // Act
            Action action = () => _ = new HooksDiscovery<YetAnotherDummy>(MockProvider<YetAnotherDummy>(new YetAnotherDummyResourceDefinition()));

            // Assert
            Assert.Throws<InvalidConfigurationException>(action);
        }

        [Fact]
        public void HookDiscovery_InheritanceWithGenericSubclass_CanDiscover()
        {
            // Act
            var hookConfig = new HooksDiscovery<AnotherDummy>(MockProvider<AnotherDummy>(new GenericDummyResourceDefinition<AnotherDummy>()));

            // Assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
        }

        private IServiceProvider MockProvider<TResource>(object service)
            where TResource : class, IIdentifiable
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => (ResourceHooksDefinition<TResource>)service);
            return services.BuildServiceProvider();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class Dummy : Identifiable
        {
            [Attr]
            public string Unused { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class DummyResourceDefinition : ResourceHooksDefinition<Dummy>
        {
            public DummyResourceDefinition()
                : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<Dummy>().Build())
            {
            }

            public override IEnumerable<Dummy> BeforeDelete(IResourceHashSet<Dummy> resources, ResourcePipeline pipeline)
            {
                return resources;
            }

            public override void AfterDelete(HashSet<Dummy> resources, ResourcePipeline pipeline, bool succeeded)
            {
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class AnotherDummy : Identifiable
        {
            [Attr]
            public string Unused { get; set; }
        }

        public abstract class ResourceDefinitionBase<T> : ResourceHooksDefinition<T>
            where T : class, IIdentifiable
        {
            protected ResourceDefinitionBase(IResourceGraph resourceGraph)
                : base(resourceGraph)
            {
            }

            public override IEnumerable<T> BeforeDelete(IResourceHashSet<T> resources, ResourcePipeline pipeline)
            {
                return resources;
            }

            public override void AfterDelete(HashSet<T> resources, ResourcePipeline pipeline, bool succeeded)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class AnotherDummyResourceDefinition : ResourceDefinitionBase<AnotherDummy>
        {
            public AnotherDummyResourceDefinition()
                : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<AnotherDummy>().Build())
            {
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class YetAnotherDummy : Identifiable
        {
            [Attr]
            public string Unused { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class YetAnotherDummyResourceDefinition : ResourceHooksDefinition<YetAnotherDummy>
        {
            public YetAnotherDummyResourceDefinition()
                : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<YetAnotherDummy>().Build())
            {
            }

            public override IEnumerable<YetAnotherDummy> BeforeDelete(IResourceHashSet<YetAnotherDummy> resources, ResourcePipeline pipeline)
            {
                return resources;
            }

            [LoadDatabaseValues(false)]
            public override void AfterDelete(HashSet<YetAnotherDummy> resources, ResourcePipeline pipeline, bool succeeded)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class GenericDummyResourceDefinition<TResource> : ResourceHooksDefinition<TResource>
            where TResource : class, IIdentifiable<int>
        {
            public GenericDummyResourceDefinition()
                : base(new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TResource>().Build())
            {
            }

            public override IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline)
            {
                return resources;
            }

            public override void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded)
            {
            }
        }
    }
}
