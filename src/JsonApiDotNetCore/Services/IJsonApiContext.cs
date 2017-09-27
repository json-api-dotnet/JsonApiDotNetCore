using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        JsonApiOptions Options { get; set; }
        IJsonApiContext ApplyContext<T>(object controller);
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
        string BasePath { get; set; }
        QuerySet QuerySet { get; set; }
        bool IsRelationshipData { get; set; }
        List<string> IncludedRelationships { get; set; }
        bool IsRelationshipPath { get; }
        PageManager PageManager { get; set; }
        IMetaBuilder MetaBuilder { get; set; }
        IGenericProcessorFactory GenericProcessorFactory { get; set; }
        Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; }
        Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; }
        Type ControllerType { get; set; }
        TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute;
        IDbContextResolver GetDbContextResolver();
    }
}
