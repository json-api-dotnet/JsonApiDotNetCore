using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.RequestAdapters;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.Deserialization
{
    public abstract class DeserializationBenchmarkBase
    {
        protected readonly JsonSerializerOptions SerializerReadOptions;
        protected readonly DocumentAdapter DocumentAdapter;

        protected DeserializationBenchmarkBase()
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<ResourceA>().Build();
            options.SerializerOptions.Converters.Add(new ResourceObjectConverter(resourceGraph));
            SerializerReadOptions = ((IJsonApiOptions)options).SerializerReadOptions;

            var serviceContainer = new ServiceContainer();
            var resourceFactory = new ResourceFactory(serviceContainer);
            var resourceDefinitionAccessor = new ResourceDefinitionAccessor(resourceGraph, serviceContainer);

            serviceContainer.AddService(typeof(IResourceDefinitionAccessor), resourceDefinitionAccessor);
            serviceContainer.AddService(typeof(IResourceDefinition<ResourceA>), new JsonApiResourceDefinition<ResourceA>(resourceGraph));

            // ReSharper disable once VirtualMemberCallInConstructor
            JsonApiRequest request = CreateJsonApiRequest(resourceGraph);
            var targetedFields = new TargetedFields();

            var resourceIdentifierObjectAdapter = new ResourceIdentifierObjectAdapter(resourceGraph, resourceFactory);
            var relationshipDataAdapter = new RelationshipDataAdapter(resourceGraph, resourceIdentifierObjectAdapter);
            var resourceObjectAdapter = new ResourceObjectAdapter(resourceGraph, resourceFactory, options, relationshipDataAdapter);
            var resourceDataAdapter = new ResourceDataAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

            var atomicReferenceAdapter = new AtomicReferenceAdapter(resourceGraph, resourceFactory);
            var atomicOperationResourceDataAdapter = new ResourceDataInOperationsRequestAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

            var atomicOperationObjectAdapter = new AtomicOperationObjectAdapter(resourceGraph, options, atomicReferenceAdapter,
                atomicOperationResourceDataAdapter, relationshipDataAdapter);

            var resourceDocumentAdapter = new DocumentInResourceOrRelationshipRequestAdapter(options, resourceDataAdapter, relationshipDataAdapter);
            var operationsDocumentAdapter = new DocumentInOperationsRequestAdapter(options, atomicOperationObjectAdapter);

            DocumentAdapter = new DocumentAdapter(request, targetedFields, resourceDocumentAdapter, operationsDocumentAdapter);
        }

        protected abstract JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph);

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class ResourceA : Identifiable
        {
            [Attr]
            public bool Attribute01 { get; set; }

            [Attr]
            public char Attribute02 { get; set; }

            [Attr]
            public ulong? Attribute03 { get; set; }

            [Attr]
            public decimal Attribute04 { get; set; }

            [Attr]
            public float? Attribute05 { get; set; }

            [Attr]
            public string Attribute06 { get; set; }

            [Attr]
            public DateTime? Attribute07 { get; set; }

            [Attr]
            public DateTimeOffset? Attribute08 { get; set; }

            [Attr]
            public TimeSpan? Attribute09 { get; set; }

            [Attr]
            public DayOfWeek Attribute10 { get; set; }

            [HasOne]
            public ResourceA Single1 { get; set; }

            [HasOne]
            public ResourceA Single2 { get; set; }

            [HasOne]
            public ResourceA Single3 { get; set; }

            [HasOne]
            public ResourceA Single4 { get; set; }

            [HasOne]
            public ResourceA Single5 { get; set; }

            [HasMany]
            public ISet<ResourceA> Multi1 { get; set; }

            [HasMany]
            public ISet<ResourceA> Multi2 { get; set; }

            [HasMany]
            public ISet<ResourceA> Multi3 { get; set; }

            [HasMany]
            public ISet<ResourceA> Multi4 { get; set; }

            [HasMany]
            public ISet<ResourceA> Multi5 { get; set; }
        }
    }
}
