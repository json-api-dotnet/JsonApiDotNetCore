using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Builders
{
    public abstract class DocumentBuilder : ResourceObjectBuilder
    {
        protected DocumentBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider) : base(resourceGraph, provider) { }
        protected Document Build(IIdentifiable entity, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
        {
            if (entity == null)
                return new Document();

            return new Document { Data = BuildResourceObject(entity, attributes, relationships) };
        }

        protected Document Build(IEnumerable entities, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
        {
            var data = new List<ResourceObject>();
            foreach (IIdentifiable entity in entities)
                data.Add(BuildResourceObject(entity, attributes, relationships));

            return new Document { Data = data };
        }

        protected string GetStringOutput(Document document)
        {
            //var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
            return JsonConvert.SerializeObject(document);
        }
    }
}
