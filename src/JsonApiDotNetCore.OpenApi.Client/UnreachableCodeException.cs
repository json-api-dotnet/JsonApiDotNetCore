namespace JsonApiDotNetCore.OpenApi.Client;

internal sealed class UnreachableCodeException : Exception
{
    public UnreachableCodeException()
        : base("This code should not be reachable.")
    {
    }
}
