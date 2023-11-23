using System.Text;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

internal sealed class AuthScopeSet
{
    private const StringSplitOptions ScopesHeaderSplitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

    public const string ScopesHeaderName = "X-Auth-Scopes";

    private readonly Dictionary<string, Permission> _scopes = [];

    public static AuthScopeSet GetRequestedScopes(IHeaderDictionary requestHeaders)
    {
        var requestedScopes = new AuthScopeSet();

        // In a real application, the scopes would be read from the signed ticket in the Authorization HTTP header.
        // For simplicity, this sample allows the client to send them directly, which is obviously insecure.

        if (requestHeaders.TryGetValue(ScopesHeaderName, out StringValues headerValue))
        {
            foreach (string scopeValue in headerValue.ToString().Split(' ', ScopesHeaderSplitOptions))
            {
                string[] scopeParts = scopeValue.Split(':', 2, ScopesHeaderSplitOptions);

                if (scopeParts.Length == 2 && Enum.TryParse(scopeParts[0], true, out Permission permission) && Enum.IsDefined(permission))
                {
                    requestedScopes.Include(scopeParts[1], permission);
                }
            }
        }

        return requestedScopes;
    }

    public void IncludeFrom(IJsonApiRequest request, ITargetedFields targetedFields)
    {
        Permission permission = request.IsReadOnly ? Permission.Read : Permission.Write;

        if (request.PrimaryResourceType != null)
        {
            Include(request.PrimaryResourceType, permission);
        }

        if (request.SecondaryResourceType != null)
        {
            Include(request.SecondaryResourceType, permission);
        }

        if (request.Relationship != null)
        {
            Include(request.Relationship, permission);
        }

        foreach (RelationshipAttribute relationship in targetedFields.Relationships)
        {
            Include(relationship, permission);
        }
    }

    public void Include(ResourceType resourceType, Permission permission)
    {
        Include(resourceType.PublicName, permission);
    }

    public void Include(RelationshipAttribute relationship, Permission permission)
    {
        Include(relationship.LeftType, permission);
        Include(relationship.RightType, permission);
    }

    private void Include(string name, Permission permission)
    {
        // Unify with existing entries. For example, adding read:movies when write:movies already exists is a no-op.

        if (_scopes.TryGetValue(name, out Permission value))
        {
            if (value >= permission)
            {
                return;
            }
        }

        _scopes[name] = permission;
    }

    public bool ContainsAll(AuthScopeSet other)
    {
        foreach (string otherName in other._scopes.Keys)
        {
            if (!_scopes.TryGetValue(otherName, out Permission thisPermission))
            {
                return false;
            }

            if (thisPermission < other._scopes[otherName])
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach ((string name, Permission permission) in _scopes.OrderBy(scope => scope.Key))
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append($"{permission.ToString().ToLowerInvariant()}:{name}");
        }

        return builder.ToString();
    }
}
