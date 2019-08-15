using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
<<<<<<< HEAD
using JsonApiDotNetCore.Internal.Contracts;
=======
>>>>>>> master
using JsonApiDotNetCore.Models;

namespace GettingStarted.ResourceDefinitionExample
{
    public class ModelDefinition : ResourceDefinition<Model>
    {
        public ModelDefinition(IResourceGraph graph) : base(graph)
        {
        }

        // this allows POST / PATCH requests to set the value of a
        // property, but we don't include this value in the response
        // this might be used if the incoming value gets hashed or
        // encrypted prior to being persisted and this value should
        // never be sent back to the client
        protected override List<AttrAttribute> OutputAttrs()
            => Remove(model => model.DontExpose);
    }
}
