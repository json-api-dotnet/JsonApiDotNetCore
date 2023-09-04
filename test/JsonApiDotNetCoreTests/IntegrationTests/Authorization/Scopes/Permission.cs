namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

internal enum Permission
{
    Read,

    // Write access implicitly includes read access, because POST/PATCH in JSON:API may return the changed resource.
    Write
}
