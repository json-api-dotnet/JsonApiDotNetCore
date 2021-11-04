using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace Benchmarks.Serialization
{
    [MarkdownExporter]
    // ReSharper disable once ClassCanBeSealed.Global
    public class ResourceSerializationBenchmarks : SerializationBenchmarkBase
    {
        private static readonly OutgoingResource ResponseResource = CreateResponseResource();

        private static OutgoingResource CreateResponseResource()
        {
            var resource1 = new OutgoingResource
            {
                Id = 1,
                Attribute01 = true,
                Attribute02 = 'A',
                Attribute03 = 100UL,
                Attribute04 = 100.001m,
                Attribute05 = 100.002f,
                Attribute06 = "text1",
                Attribute07 = new DateTime(2001, 1, 1),
                Attribute08 = new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)),
                Attribute09 = new TimeSpan(1, 0, 0),
                Attribute10 = DayOfWeek.Sunday
            };

            var resource2 = new OutgoingResource
            {
                Id = 2,
                Attribute01 = false,
                Attribute02 = 'B',
                Attribute03 = 200UL,
                Attribute04 = 200.001m,
                Attribute05 = 200.002f,
                Attribute06 = "text2",
                Attribute07 = new DateTime(2002, 2, 2),
                Attribute08 = new DateTimeOffset(2002, 2, 2, 0, 0, 0, TimeSpan.FromHours(2)),
                Attribute09 = new TimeSpan(2, 0, 0),
                Attribute10 = DayOfWeek.Monday
            };

            var resource3 = new OutgoingResource
            {
                Id = 3,
                Attribute01 = true,
                Attribute02 = 'C',
                Attribute03 = 300UL,
                Attribute04 = 300.001m,
                Attribute05 = 300.002f,
                Attribute06 = "text3",
                Attribute07 = new DateTime(2003, 3, 3),
                Attribute08 = new DateTimeOffset(2003, 3, 3, 0, 0, 0, TimeSpan.FromHours(3)),
                Attribute09 = new TimeSpan(3, 0, 0),
                Attribute10 = DayOfWeek.Tuesday
            };

            var resource4 = new OutgoingResource
            {
                Id = 4,
                Attribute01 = false,
                Attribute02 = 'D',
                Attribute03 = 400UL,
                Attribute04 = 400.001m,
                Attribute05 = 400.002f,
                Attribute06 = "text4",
                Attribute07 = new DateTime(2004, 4, 4),
                Attribute08 = new DateTimeOffset(2004, 4, 4, 0, 0, 0, TimeSpan.FromHours(4)),
                Attribute09 = new TimeSpan(4, 0, 0),
                Attribute10 = DayOfWeek.Wednesday
            };

            var resource5 = new OutgoingResource
            {
                Id = 5,
                Attribute01 = true,
                Attribute02 = 'E',
                Attribute03 = 500UL,
                Attribute04 = 500.001m,
                Attribute05 = 500.002f,
                Attribute06 = "text5",
                Attribute07 = new DateTime(2005, 5, 5),
                Attribute08 = new DateTimeOffset(2005, 5, 5, 0, 0, 0, TimeSpan.FromHours(5)),
                Attribute09 = new TimeSpan(5, 0, 0),
                Attribute10 = DayOfWeek.Thursday
            };

            resource1.Single2 = resource2;
            resource2.Single3 = resource3;
            resource3.Multi4 = resource4.AsHashSet();
            resource4.Multi5 = resource5.AsHashSet();

            return resource1;
        }

        [Benchmark]
        public string SerializeResourceResponse()
        {
            Document responseDocument = ResponseModelAdapter.Convert(ResponseResource);
            return JsonSerializer.Serialize(responseDocument, SerializerWriteOptions);
        }

        protected override JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph)
        {
            return new JsonApiRequest
            {
                Kind = EndpointKind.Primary,
                PrimaryResourceType = resourceGraph.GetResourceType<OutgoingResource>()
            };
        }

        protected override IEvaluatedIncludeCache CreateEvaluatedIncludeCache(IResourceGraph resourceGraph)
        {
            ResourceType resourceAType = resourceGraph.GetResourceType<OutgoingResource>();

            RelationshipAttribute single2 = resourceAType.GetRelationshipByPropertyName(nameof(OutgoingResource.Single2));
            RelationshipAttribute single3 = resourceAType.GetRelationshipByPropertyName(nameof(OutgoingResource.Single3));
            RelationshipAttribute multi4 = resourceAType.GetRelationshipByPropertyName(nameof(OutgoingResource.Multi4));
            RelationshipAttribute multi5 = resourceAType.GetRelationshipByPropertyName(nameof(OutgoingResource.Multi5));

            ImmutableArray<ResourceFieldAttribute> chain = ImmutableArray.Create<ResourceFieldAttribute>(single2, single3, multi4, multi5);
            IEnumerable<ResourceFieldChainExpression> chains = new ResourceFieldChainExpression(chain).AsEnumerable();

            var converter = new IncludeChainConverter();
            IncludeExpression include = converter.FromRelationshipChains(chains);

            var cache = new EvaluatedIncludeCache();
            cache.Set(include);
            return cache;
        }
    }
}
