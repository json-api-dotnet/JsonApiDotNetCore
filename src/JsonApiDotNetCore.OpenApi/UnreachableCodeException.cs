using System;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class UnreachableCodeException : Exception
    {
        public UnreachableCodeException()
            : base("This code should not be reachable.")
        {
        }
    }
}
