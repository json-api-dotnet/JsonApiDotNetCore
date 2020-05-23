using System;

namespace JsonApiDotNetCore.Models.Fluent
{
    public class EagerLoad
    {
        private EagerLoadAttribute _attribute;

        public EagerLoad(EagerLoadAttribute attribute)
        {
            _attribute = attribute;
        }
    }
}
