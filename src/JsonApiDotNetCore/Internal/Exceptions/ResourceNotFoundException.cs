using System;

namespace JsonApiDotNetCore.Internal
{
    public class ResourceNotFoundException : Exception
    {
        private readonly ErrorCollection _errors = new ErrorCollection();

        public ResourceNotFoundException()
        { }

    }
}
